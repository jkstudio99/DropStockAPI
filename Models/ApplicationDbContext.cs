using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DropStockAPI.Models;

public partial class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CategoryModel> CategoryModels { get; set; }

    public virtual DbSet<CustomerModel> CustomerModels { get; set; }

    public virtual DbSet<OrderModel> OrderModels { get; set; }

    public virtual DbSet<PaymentModel> PaymentModels { get; set; }

    public virtual DbSet<ProductModel> ProductModels { get; set; }

    public virtual DbSet<SupplierModel> SupplierModels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<CategoryModel>(entity =>
        {
            entity.HasKey(e => e.categoryid).HasName("category_pkey");

            entity.ToTable("CategoryModel");

            entity.Property(e => e.categoryid).HasDefaultValueSql("nextval('category_categoryid_seq'::regclass)");
            entity.Property(e => e.categoryname).HasMaxLength(64);
        });

        modelBuilder.Entity<CustomerModel>(entity =>
        {
            entity.HasKey(e => e.customerid).HasName("customer_pkey");

            entity.ToTable("CustomerModel");

            entity.Property(e => e.customerid).HasDefaultValueSql("nextval('customer_customerid_seq'::regclass)");
            entity.Property(e => e.address).HasMaxLength(40);
            entity.Property(e => e.email).HasMaxLength(40);
            entity.Property(e => e.firstname).HasMaxLength(40);
            entity.Property(e => e.lastname).HasMaxLength(40);
            entity.Property(e => e.phone).HasMaxLength(15);
        });

        modelBuilder.Entity<OrderModel>(entity =>
        {
            entity.HasKey(e => e.orderid).HasName("order_pkey");

            entity.ToTable("OrderModel");

            entity.Property(e => e.orderid).HasDefaultValueSql("nextval('order_orderid_seq'::regclass)");
            entity.Property(e => e.createddate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.modifieddate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.orderdetails).HasMaxLength(255);
            entity.Property(e => e.ordername).HasMaxLength(255);
            entity.Property(e => e.orderprice).HasPrecision(10, 2);
            entity.Property(e => e.orderstatus).HasMaxLength(50);
        });

        modelBuilder.Entity<PaymentModel>(entity =>
        {
            entity.HasKey(e => e.billnumber).HasName("payment_pkey");

            entity.ToTable("PaymentModel");

            entity.Property(e => e.billnumber).HasDefaultValueSql("nextval('payment_billnumber_seq'::regclass)");
            entity.Property(e => e.otherdetail).HasMaxLength(40);
            entity.Property(e => e.paymenttype).HasMaxLength(40);
        });

        modelBuilder.Entity<ProductModel>(entity =>
        {
            entity.HasKey(e => e.productid).HasName("product_pkey");

            entity.ToTable("ProductModel");

            entity.Property(e => e.productid).HasDefaultValueSql("nextval('product_productid_seq'::regclass)");
            entity.Property(e => e.createddate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.modifieddate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.productname).HasMaxLength(128);
            entity.Property(e => e.productpicture).HasMaxLength(256);
            entity.Property(e => e.unitprice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<SupplierModel>(entity =>
        {
            entity.HasKey(e => e.supplierid).HasName("supplier_pkey");

            entity.ToTable("SupplierModel");

            entity.Property(e => e.supplierid).HasDefaultValueSql("nextval('supplier_supplierid_seq'::regclass)");
            entity.Property(e => e.address).HasMaxLength(40);
            entity.Property(e => e.email).HasMaxLength(40);
            entity.Property(e => e.name).HasMaxLength(40);
            entity.Property(e => e.otherdetail).HasMaxLength(40);
            entity.Property(e => e.phone).HasMaxLength(15);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
