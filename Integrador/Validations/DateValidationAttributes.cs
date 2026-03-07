using System;
using System.ComponentModel.DataAnnotations;

namespace Integrador.Validations
{
    /// <summary>
    /// Valida que una fecha no sea posterior a la fecha actual
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NotFutureDateAttribute : ValidationAttribute
    {
        public NotFutureDateAttribute() : base("La fecha no puede ser posterior a la fecha actual")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime fecha;
            if (value is DateTime)
            {
                fecha = (DateTime)value;
            }
            else if (value is DateTime?)
            {
                var nullableDate = (DateTime?)value;
                if (!nullableDate.HasValue)
                    return ValidationResult.Success;
                fecha = nullableDate.Value;
            }
            else
            {
                return new ValidationResult("Valor de fecha inválido");
            }

            if (fecha.Date > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha no puede ser posterior a la fecha actual");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Valida que una fecha de nacimiento esté dentro de un rango razonable
    /// Para personas: no mayor a 120 años
    /// Para mascotas: no mayor a 30 años (usar MaxYearsAgo = 30)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class BirthDateRangeAttribute : ValidationAttribute
    {
        /// <summary>
        /// Máximo de años en el pasado (por defecto 120 para personas)
        /// </summary>
        public int MaxYearsAgo { get; set; } = 120;

        /// <summary>
        /// Si es true, permite fechas futuras (por defecto false)
        /// </summary>
        public bool AllowFuture { get; set; } = false;

        public BirthDateRangeAttribute() : base("La fecha de nacimiento no es válida")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime fecha;
            if (value is DateTime)
            {
                fecha = (DateTime)value;
            }
            else if (value is DateTime?)
            {
                var nullableDate = (DateTime?)value;
                if (!nullableDate.HasValue)
                    return ValidationResult.Success;
                fecha = nullableDate.Value;
            }
            else
            {
                return new ValidationResult("Valor de fecha inválido");
            }

            // Validar que no sea futura
            if (!AllowFuture && fecha.Date > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha de nacimiento no puede ser posterior a hoy");
            }

            // Validar que no sea muy antigua
            var fechaMinima = DateTime.Today.AddYears(-MaxYearsAgo);
            if (fecha.Date < fechaMinima)
            {
                return new ValidationResult(ErrorMessage ?? $"La fecha de nacimiento no puede ser anterior a {MaxYearsAgo} años atrás");
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Valida que una fecha de fin sea mayor o igual a una fecha de inicio
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DateRangeValidationAttribute : ValidationAttribute
    {
        private readonly string _startDateProperty;
        private readonly string _endDateProperty;

        public DateRangeValidationAttribute(string startDateProperty, string endDateProperty)
            : base("La fecha de fin debe ser mayor o igual a la fecha de inicio")
        {
            _startDateProperty = startDateProperty;
            _endDateProperty = endDateProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var type = value.GetType();
            var startProperty = type.GetProperty(_startDateProperty);
            var endProperty = type.GetProperty(_endDateProperty);

            if (startProperty == null || endProperty == null)
                return ValidationResult.Success;

            var startValue = startProperty.GetValue(value) as DateTime?;
            var endValue = endProperty.GetValue(value) as DateTime?;

            // Si no es nullable, intentar obtener como DateTime
            if (startValue == null && startProperty.PropertyType == typeof(DateTime))
            {
                startValue = (DateTime?)startProperty.GetValue(value);
            }
            if (endValue == null && endProperty.PropertyType == typeof(DateTime))
            {
                endValue = (DateTime?)endProperty.GetValue(value);
            }

            // Si alguna fecha es null, no validar
            if (!startValue.HasValue || !endValue.HasValue)
                return ValidationResult.Success;

            if (endValue.Value < startValue.Value)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha de fin debe ser mayor o igual a la fecha de inicio", 
                    new[] { _endDateProperty });
            }

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Valida que la persona sea mayor de edad (mínimo 18 años) y no supere 70 años.
    /// La fecha de nacimiento no puede ser hoy ni futura.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EdadEntreAttribute : ValidationAttribute
    {
        public int EdadMinima { get; set; } = 18;
        public int EdadMaxima { get; set; } = 70;

        public EdadEntreAttribute() : base("Debe tener entre 18 y 70 años.")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime fecha;
            if (value is DateTime)
                fecha = (DateTime)value;
            else if (value is DateTime?)
            {
                var nullableDate = (DateTime?)value;
                if (!nullableDate.HasValue)
                    return ValidationResult.Success;
                fecha = nullableDate.Value;
            }
            else
                return new ValidationResult("Valor de fecha inválido");

            var hoy = DateTime.Today;
            if (fecha.Date >= hoy)
                return new ValidationResult(ErrorMessage ?? "La fecha de nacimiento no puede ser hoy ni futura.");

            var fechaMaximaPermitida = hoy.AddYears(-EdadMinima);
            var fechaMinimaPermitida = hoy.AddYears(-EdadMaxima);

            if (fecha.Date > fechaMaximaPermitida)
                return new ValidationResult(ErrorMessage ?? "Debe ser mayor de edad (al menos 18 años).");

            if (fecha.Date < fechaMinimaPermitida)
                return new ValidationResult(ErrorMessage ?? "La edad no puede superar los 70 años.");

            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Valida que una fecha de reporte no sea futura y no sea muy antigua
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ReportDateAttribute : ValidationAttribute
    {
        /// <summary>
        /// Máximo de días en el pasado que se permite (por defecto 365)
        /// </summary>
        public int MaxDaysAgo { get; set; } = 365;

        public ReportDateAttribute() : base("La fecha de reporte no es válida")
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            DateTime fecha;
            if (value is DateTime)
            {
                fecha = (DateTime)value;
            }
            else if (value is DateTime?)
            {
                var nullableDate = (DateTime?)value;
                if (!nullableDate.HasValue)
                    return ValidationResult.Success;
                fecha = nullableDate.Value;
            }
            else
            {
                return new ValidationResult("Valor de fecha inválido");
            }

            // Validar que no sea futura
            if (fecha.Date > DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha de reporte no puede ser posterior a hoy");
            }

            // Validar que no sea muy antigua
            var fechaMinima = DateTime.Today.AddDays(-MaxDaysAgo);
            if (fecha.Date < fechaMinima)
            {
                return new ValidationResult(ErrorMessage ?? $"La fecha de reporte no puede ser anterior a {MaxDaysAgo} días atrás");
            }

            return ValidationResult.Success;
        }
    }
}
