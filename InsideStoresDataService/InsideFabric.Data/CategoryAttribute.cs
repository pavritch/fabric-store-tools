using System;

namespace InsideFabric.Data
{
    [AttributeUsage(AttributeTargets.All)]
    public class CategoryAttribute : Attribute
    {
        public int Id { get; set; }
        public bool Included { get; set; }
        public CategoryAttribute(int id, bool included) { Id = id; Included = included; }
    }
}