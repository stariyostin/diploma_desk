﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace diploma_desk
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PapirusEntities1 : DbContext
    {
        public PapirusEntities1()
            : base("name=PapirusEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Manager> Manager { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Order_Product> Order_Product { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<User> User { get; set; }
    }
}
