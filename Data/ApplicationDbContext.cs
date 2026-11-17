
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateAgency.Models;
using RealEstateAgency.Models.Profiles;

namespace RealEstateAgency.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();
    public DbSet<RealtorProfile> RealtorProfiles => Set<RealtorProfile>();
    public DbSet<AdminProfile> AdminProfiles => Set<AdminProfile>();

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<PropertyPhoto> PropertyPhotos => Set<PropertyPhoto>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Selection> Selections => Set<Selection>();
    public DbSet<Contract> Contracts => Set<Contract>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Связь пользователя с профилем клиента: один к одному.
        builder.Entity<ClientProfile>()
            .HasOne(profile => profile.User)
            .WithOne()
            .HasForeignKey<ClientProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь пользователя с профилем риелтора: один к одному.
        builder.Entity<RealtorProfile>()
            .HasOne(profile => profile.User)
            .WithOne()
            .HasForeignKey<RealtorProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Связь пользователя с профилем администратора: один к одному.
        builder.Entity<AdminProfile>()
            .HasOne(profile => profile.User)
            .WithOne()
            .HasForeignKey<AdminProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один риелтор может вести несколько объектов.
        builder.Entity<Property>()
            .HasOne(property => property.Realtor)
            .WithMany(realtor => realtor.Properties)
            .HasForeignKey(property => property.RealtorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // У одного объекта может быть несколько фотографий.
        builder.Entity<PropertyPhoto>()
            .HasOne(photo => photo.Property)
            .WithMany(property => property.Photos)
            .HasForeignKey(photo => photo.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один клиент может создать несколько заявок.
        builder.Entity<Request>()
            .HasOne(request => request.Client)
            .WithMany(client => client.Requests)
            .HasForeignKey(request => request.ClientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Риелтор может быть назначен на несколько заявок.
        // До назначения риелтора поле RealtorProfileId может быть пустым.
        builder.Entity<Request>()
            .HasOne(request => request.Realtor)
            .WithMany(realtor => realtor.Requests)
            .HasForeignKey(request => request.RealtorProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        // Заявка может содержать несколько вариантов недвижимости.
        builder.Entity<Selection>()
            .HasOne(selection => selection.Request)
            .WithMany(request => request.Selections)
            .HasForeignKey(selection => selection.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один объект может предлагаться в разных подборках.
        builder.Entity<Selection>()
            .HasOne(selection => selection.Property)
            .WithMany(property => property.Selections)
            .HasForeignKey(selection => selection.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Один клиент может заключить несколько договоров.
        builder.Entity<Contract>()
            .HasOne(contract => contract.Client)
            .WithMany(client => client.Contracts)
            .HasForeignKey(contract => contract.ClientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Один риелтор может сопровождать несколько договоров.
        builder.Entity<Contract>()
            .HasOne(contract => contract.Realtor)
            .WithMany()
            .HasForeignKey(contract => contract.RealtorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Для одного объекта недвижимости допускается один договор.
        builder.Entity<Contract>()
            .HasOne(contract => contract.Property)
            .WithOne(property => property.Contract)
            .HasForeignKey<Contract>(contract => contract.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ограничения уникальности.
        builder.Entity<Contract>()
            .HasIndex(contract => contract.ContractNumber)
            .IsUnique();

        builder.Entity<Selection>()
            .HasIndex(selection => new
            {
                selection.RequestId,
                selection.PropertyId
            })
            .IsUnique();

        // Индексы для поиска недвижимости.
        builder.Entity<Property>()
            .HasIndex(property => new
            {
                property.District,
                property.Type,
                property.Status
            });

        builder.Entity<Property>()
            .HasIndex(property => property.Price);

        // Индексы для поиска и обработки заявок.
        builder.Entity<Request>()
            .HasIndex(request => request.Status);

        builder.Entity<Request>()
            .HasIndex(request => request.CreatedAt);

        // Настройка точности денежных и числовых значений.
        builder.Entity<Property>()
            .Property(property => property.Area)
            .HasPrecision(10, 2);

        builder.Entity<Property>()
            .Property(property => property.Price)
            .HasPrecision(18, 2);

        builder.Entity<Request>()
            .Property(request => request.MinPrice)
            .HasPrecision(18, 2);

        builder.Entity<Request>()
            .Property(request => request.MaxPrice)
            .HasPrecision(18, 2);

        builder.Entity<Request>()
            .Property(request => request.MinArea)
            .HasPrecision(10, 2);

        builder.Entity<Request>()
            .Property(request => request.MaxArea)
            .HasPrecision(10, 2);

        builder.Entity<Contract>()
            .Property(contract => contract.PropertyPrice)
            .HasPrecision(18, 2);

        builder.Entity<Contract>()
            .Property(contract => contract.CommissionPercent)
            .HasPrecision(5, 2);

        builder.Entity<Contract>()
            .Property(contract => contract.CommissionAmount)
            .HasPrecision(18, 2);

        builder.Entity<RealtorProfile>()
            .Property(realtor => realtor.CommissionPercent)
            .HasPrecision(5, 2);

        builder.Entity<ClientProfile>()
            .Property(client => client.Budget)
            .HasPrecision(18, 2);
    }
}
