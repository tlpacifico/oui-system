using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Application.Pos;
using shs.Api.IntegrationTests.Infrastructure;
using shs.Domain.Entities;
using shs.Domain.Enums;
using Xunit;

namespace shs.Api.IntegrationTests.Tests.Pos;

public class ProcessSaleTests : IntegrationTestBase
{
    // The ProcessSale endpoint resolves UserId from NameIdentifier/user_id/sub claims.
    // TestAuthHandler only sets email claims, so UserId falls back to "unknown".
    private const string TestUserId = "unknown";

    public ProcessSaleTests(PostgresContainerFixture dbFixture) : base(dbFixture) { }

    #region Cenário 1: Cliente comum + Peça própria (€10,00)

    [Fact]
    public async Task ProcessSale_OwnPurchase_CashPayment_ShouldSucceed()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("TestBrand");
        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplierId: null, AcquisitionType.OwnPurchase,
            evaluatedPrice: 10m, costPrice: 4m);
        var registerExternalId = await SeedOpenCashRegister();

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new[] { new { Method = "Cash", Amount = 10m, Reference = (string?)null, SupplierId = (long?)null } },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sale = await response.Content.ReadFromJsonAsync<ProcessSaleResponse>();
        sale.Should().NotBeNull();
        sale!.TotalAmount.Should().Be(10m);
        sale.ItemCount.Should().Be(1);

        // Verify item status changed to Sold
        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.Status.Should().Be(ItemStatus.Sold);
        item.CommissionAmount.Should().BeNull();
    }

    #endregion

    #region Cenário 2a: Consignação + Fornecedor optou crédito (€20,00)

    [Fact]
    public async Task ProcessSale_Consignment_SupplierOptsCredit_ShouldCalculateCommission()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("Brand2a");
        var supplier = await SeedSupplier("Maria", creditPercentageInStore: 50m, cashRedemptionPercentage: 0m);
        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplier.Id, AcquisitionType.Consignment,
            evaluatedPrice: 20m, costPrice: null);
        var registerExternalId = await SeedOpenCashRegister();

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new[] { new { Method = "CreditCard", Amount = 20m, Reference = (string?)null, SupplierId = (long?)null } },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sale = await response.Content.ReadFromJsonAsync<ProcessSaleResponse>();
        sale!.TotalAmount.Should().Be(20m);

        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.Status.Should().Be(ItemStatus.Sold);
        item.CommissionAmount.Should().Be(10m); // 20 * 50%
        item.CommissionPercentage.Should().Be(50m);
    }

    #endregion

    #region Cenário 2b: Consignação + Fornecedor optou dinheiro (€20,00)

    [Fact]
    public async Task ProcessSale_Consignment_SupplierOptsCash_ShouldCalculateCommission()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("Brand2b");
        var supplier = await SeedSupplier("Maria", creditPercentageInStore: 0m, cashRedemptionPercentage: 40m);
        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplier.Id, AcquisitionType.Consignment,
            evaluatedPrice: 20m, costPrice: null);
        var registerExternalId = await SeedOpenCashRegister();

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new[] { new { Method = "Cash", Amount = 20m, Reference = (string?)null, SupplierId = (long?)null } },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sale = await response.Content.ReadFromJsonAsync<ProcessSaleResponse>();
        sale!.TotalAmount.Should().Be(20m);

        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.Status.Should().Be(ItemStatus.Sold);
        item.CommissionAmount.Should().Be(8m); // 20 * 40%
        item.CommissionPercentage.Should().Be(40m);
    }

    #endregion

    #region Cenário 3: Fornecedor compra peça própria da loja (€15,00)

    [Fact]
    public async Task ProcessSale_SupplierBuysOwnPurchaseItem_NoCommission()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("Brand3");
        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplierId: null, AcquisitionType.OwnPurchase,
            evaluatedPrice: 15m, costPrice: 6m);
        var registerExternalId = await SeedOpenCashRegister();

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new[] { new { Method = "CreditCard", Amount = 15m, Reference = (string?)null, SupplierId = (long?)null } },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var sale = await response.Content.ReadFromJsonAsync<ProcessSaleResponse>();
        sale!.TotalAmount.Should().Be(15m);

        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.Status.Should().Be(ItemStatus.Sold);
        item.CommissionAmount.Should().BeNull();
    }

    #endregion

    #region Cenário 4.1: Fornecedor compra peça de outro fornecedor + Pagamento normal (€25,00)

    [Fact]
    public async Task ProcessSale_SupplierBuysFromOtherSupplier_NormalPayment()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("Brand41");
        var supplierAna = await SeedSupplier("Ana", creditPercentageInStore: 50m, cashRedemptionPercentage: 0m);
        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplierAna.Id, AcquisitionType.Consignment,
            evaluatedPrice: 25m, costPrice: null);
        var registerExternalId = await SeedOpenCashRegister();

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new[] { new { Method = "Cash", Amount = 25m, Reference = (string?)null, SupplierId = (long?)null } },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.Status.Should().Be(ItemStatus.Sold);
        item.CommissionAmount.Should().Be(12.5m); // 25 * 50%
        item.CommissionPercentage.Should().Be(50m);
    }

    #endregion

    #region Cenário 4.2: Fornecedor paga com crédito (€20,00)

    [Fact]
    public async Task ProcessSale_SupplierPaysWithStoreCredit_ShouldDeductBalance()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("Brand42");
        var supplierAna = await SeedSupplier("Ana", creditPercentageInStore: 50m, cashRedemptionPercentage: 0m);
        var supplierMaria = await SeedSupplier("Maria42", creditPercentageInStore: 50m, cashRedemptionPercentage: 0m);

        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplierAna.Id, AcquisitionType.Consignment,
            evaluatedPrice: 20m, costPrice: null);
        var registerExternalId = await SeedOpenCashRegister();

        // Maria has €25 store credit
        await SeedStoreCredit(supplierMaria.Id, originalAmount: 25m, currentBalance: 25m);

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new[] { new { Method = "StoreCredit", Amount = 20m, Reference = (string?)null, SupplierId = (long?)supplierMaria.Id } },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify item commission
        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.CommissionAmount.Should().Be(10m); // 20 * 50%

        // Verify Maria's store credit balance reduced from 25 to 5
        await using var salesDb = CreateSalesDbContext();
        var credit = await salesDb.StoreCredits.FirstAsync(sc => sc.SupplierId == supplierMaria.Id);
        credit.CurrentBalance.Should().Be(5m);
        credit.Status.Should().Be(StoreCreditStatus.Active);

        // Verify StoreCreditTransaction was created
        var transaction = await salesDb.StoreCreditTransactions
            .FirstOrDefaultAsync(t => t.StoreCreditId == credit.Id);
        transaction.Should().NotBeNull();
        transaction!.Amount.Should().Be(-20m);
        transaction.BalanceAfter.Should().Be(5m);
    }

    #endregion

    #region Cenário 4.3: Pagamento misto crédito + cartão (€30,00)

    [Fact]
    public async Task ProcessSale_MixedPayment_StoreCreditAndCard_ShouldDeductCredit()
    {
        // Arrange
        var client = Factory.CreateAuthenticatedClient();
        var brand = await SeedBrand("Brand43");
        var supplierAna = await SeedSupplier("Ana43", creditPercentageInStore: 0m, cashRedemptionPercentage: 40m);
        var supplierMaria = await SeedSupplier("Maria43", creditPercentageInStore: 50m, cashRedemptionPercentage: 0m);

        var itemExternalId = Guid.NewGuid();
        await SeedItem(itemExternalId, brand.Id, supplierAna.Id, AcquisitionType.Consignment,
            evaluatedPrice: 30m, costPrice: null);
        var registerExternalId = await SeedOpenCashRegister();

        // Maria has €12 store credit
        await SeedStoreCredit(supplierMaria.Id, originalAmount: 12m, currentBalance: 12m);

        var request = new
        {
            CashRegisterId = registerExternalId,
            Items = new[] { new { ItemExternalId = itemExternalId, DiscountAmount = 0m } },
            Payments = new object[]
            {
                new { Method = "StoreCredit", Amount = 12m, Reference = (string?)null, SupplierId = (long?)supplierMaria.Id },
                new { Method = "CreditCard", Amount = 18m, Reference = (string?)null, SupplierId = (long?)null }
            },
            DiscountPercentage = (decimal?)null,
            DiscountReason = (string?)null,
            CustomerExternalId = (Guid?)null,
            Notes = (string?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/pos/sales", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify item commission (Ana: 40% cash)
        await using var inventoryDb = CreateInventoryDbContext();
        var item = await inventoryDb.Items.FirstAsync(i => i.ExternalId == itemExternalId);
        item.CommissionAmount.Should().Be(12m); // 30 * 40%
        item.CommissionPercentage.Should().Be(40m);

        // Verify Maria's store credit fully used
        await using var salesDb = CreateSalesDbContext();
        var credit = await salesDb.StoreCredits.FirstAsync(sc => sc.SupplierId == supplierMaria.Id);
        credit.CurrentBalance.Should().Be(0m);
        credit.Status.Should().Be(StoreCreditStatus.FullyUsed);
    }

    #endregion

    #region Seed Helpers

    private async Task<BrandEntity> SeedBrand(string name)
    {
        await using var db = CreateInventoryDbContext();
        var brand = new BrandEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = name,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.Brands.Add(brand);
        await db.SaveChangesAsync();
        return brand;
    }

    private async Task<SupplierEntity> SeedSupplier(
        string name,
        decimal creditPercentageInStore,
        decimal cashRedemptionPercentage)
    {
        await using var db = CreateInventoryDbContext();
        var supplier = new SupplierEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = name,
            Email = $"{name.ToLower()}@test.com",
            PhoneNumber = "+351900000000",
            Initial = name[..1].ToUpper(),
            CreditPercentageInStore = creditPercentageInStore,
            CashRedemptionPercentage = cashRedemptionPercentage,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync();

        return supplier;
    }

    private async Task SeedItem(
        Guid externalId,
        long brandId,
        long? supplierId,
        AcquisitionType acquisitionType,
        decimal evaluatedPrice,
        decimal? costPrice)
    {
        await using var db = CreateInventoryDbContext();
        var item = new ItemEntity
        {
            ExternalId = externalId,
            IdentificationNumber = $"T{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            Name = "Test Item",
            BrandId = brandId,
            Size = "M",
            Color = "Black",
            Condition = ItemCondition.Good,
            EvaluatedPrice = evaluatedPrice,
            CostPrice = costPrice,
            Status = ItemStatus.ToSell,
            AcquisitionType = acquisitionType,
            Origin = acquisitionType == AcquisitionType.Consignment ? ItemOrigin.Consignment : ItemOrigin.Other,
            SupplierId = supplierId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedOpenCashRegister()
    {
        await using var db = CreateSalesDbContext();
        var externalId = Guid.NewGuid();
        var register = new CashRegisterEntity
        {
            ExternalId = externalId,
            OperatorUserId = TestUserId,
            OperatorName = "Test Operator",
            RegisterNumber = Random.Shared.Next(1, 100),
            OpenedAt = DateTime.UtcNow,
            OpeningAmount = 100m,
            Status = CashRegisterStatus.Open,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.CashRegisters.Add(register);
        await db.SaveChangesAsync();
        return externalId;
    }

    private async Task SeedStoreCredit(long supplierId, decimal originalAmount, decimal currentBalance)
    {
        await using var db = CreateSalesDbContext();
        var credit = new StoreCreditEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = supplierId,
            OriginalAmount = originalAmount,
            CurrentBalance = currentBalance,
            IssuedOn = DateTime.UtcNow.AddDays(-30),
            Status = StoreCreditStatus.Active,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = TestUserId
        };
        db.StoreCredits.Add(credit);
        await db.SaveChangesAsync();
    }

    #endregion
}
