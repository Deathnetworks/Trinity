﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Trinity.UIComponents
{
    public class TVarValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            BindingGroup bindingGroup = (BindingGroup)value;
            KeyValuePair<string, TVar> kvp = (KeyValuePair<string, TVar>)bindingGroup.Items[0];

            TVar tvar = kvp.Value;

            if (tvar.DefaultValue.GetType() == typeof(int))
            {
                try
                {
                    int val = Int32.Parse(tvar.Value.ToString());
                }
                catch
                {
                    return new ValidationResult(false, "Value is not an int");
                }
            }

            if (tvar.DefaultValue.GetType() == typeof(bool))
            {
                try
                {
                    bool val = Boolean.Parse(tvar.Value.ToString());
                }
                catch 
                {
                    return new ValidationResult(false, "Value is not a bool");
                }
            }

            if (tvar.DefaultValue.GetType() == typeof(float))
            {
                try
                {
                    float val = Single.Parse(tvar.Value.ToString());
                }
                catch 
                {
                    return new ValidationResult(false, "Value is not a float");
                }
            }

            if (tvar.DefaultValue.GetType() == typeof(double))
            {
                try
                {
                    double val = Double.Parse(tvar.Value.ToString());
                }
                catch 
                {
                    return new ValidationResult(false, "Value is not a double");
                }
            }
            

            return ValidationResult.ValidResult;
            
        }
    }
}
