using System.ComponentModel.DataAnnotations;

namespace CatManager.Models
{
    public class Cat
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required(ErrorMessage = "La raza es obligatoria")]
        [Display(Name = "Raza")]
        public string Breed { get; set; }

        [Required(ErrorMessage = "La edad es obligatoria")]
        [Range(0, 30, ErrorMessage = "La edad debe estar entre 0 y 30 años")]
        [Display(Name = "Edad")]
        public int Age { get; set; }

        [Display(Name = "Descripción")]
        public string Description { get; set; }

        [Display(Name = "Foto")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "El color es obligatorio")]
        [Display(Name = "Color")]
        public string Color { get; set; }

        [Display(Name = "Disponible para adopción")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Es amigable con niños")]
        public bool IsKidFriendly { get; set; }
    }
}
