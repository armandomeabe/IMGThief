﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IMGBase64URLExtractor
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class EmailSeekerEntities : DbContext
    {
        public EmailSeekerEntities()
            : base("name=EmailSeekerEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Email> Emails { get; set; }
        public virtual DbSet<Sitio> Sitios { get; set; }
        public virtual DbSet<URL> URLs { get; set; }
    }
}
