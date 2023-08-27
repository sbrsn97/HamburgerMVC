﻿using Hamburger.DAL.EntityConfigurations;
using Hamburger.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hamburger.DAL
{
    public class Context:IdentityDbContext<User,Role,int>
	{
        public Context(DbContextOptions<Context> options):base(options)
        {
            
        }

        DbSet<Category> Categories { get; set; }
        DbSet<Menu> Menus { get; set; }
        DbSet<Order> Orders { get; set; }
        DbSet<OrderDetails> OrderDetails { get; set; }
        DbSet<Product> Products { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<Size> Sizes { get; set; }
        DbSet<Status> Statuses { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<MenuProduct> MenuProducts { get; set; }
        DbSet<Topping> Toppings { get; set; }
        DbSet<ProductTopping> ProductToppings { get; set; }
		protected override async void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration<User>(new UserCFG());
            builder.ApplyConfiguration<Role>(new RoleCFG());
            builder.ApplyConfiguration<Category>(new CategoryCFG());
            builder.ApplyConfiguration<Product>(new ProductCFG());
            builder.ApplyConfiguration<Size>(new SizeCFG());

            builder.Entity<Role>().HasData(
                    new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                    new Role { Id = 2, Name = "Standard", NormalizedName = "STANDARD"}
                );
            
            List<User> admins = new List<User>() { new User { Id = 1, FirstName = "overthinkers", LastName = "team", Email = "overthinkerst@gmail.com", NormalizedEmail = "OVERTHINKERST@GMAIL.COM", UserName = "overthinkers", NormalizedUserName = "OVERTHINKERS", Address="Kadikoy" } };

            builder.Entity<User>().HasData(admins);

            var passwordHasher = new PasswordHasher<User>();
            admins[0].PasswordHash = passwordHasher.HashPassword(admins[0],"Overthinkers2000");

            builder.Entity<IdentityUserRole<int>>().HasData(
                    new IdentityUserRole<int>{ RoleId =1, UserId = 1}
                );

			base.OnModelCreating(builder);
		}
	}
}
