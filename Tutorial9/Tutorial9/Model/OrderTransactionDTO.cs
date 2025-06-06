﻿using System.ComponentModel.DataAnnotations;

namespace Tutorial9.Model;

public class OrderTransactionDTO
{
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public int IdWarehouse { get; set; }
    [Required]
    public int Ammount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}