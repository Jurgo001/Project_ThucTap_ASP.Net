using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCrud.DataServices.Models;

public class CategoryDTO
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}