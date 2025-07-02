using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EvolutiveSystem_02
{
    /// <summary>
    /// BNuova classe per il bindig dei metodi fornita da Gemini
    /// </summary>
    class ClsCustomBinder : Binder
    {
        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            foreach (FieldInfo fi in match)
            {
                // Controlla se il tipo del campo è assegnabile dal tipo del valore
                if (fi.FieldType.IsAssignableFrom(value.GetType()))
                {
                    return fi;
                }
            }
            return null;
        }

        public override MethodBase BindToMethod(
            BindingFlags bindingAttr,
            MethodBase[] match,
            ref object[] args,
            ParameterModifier[] modifiers,
            CultureInfo culture,
            string[] names,
            out object state)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            state = null; // Arguments are not being reordered.

            foreach (MethodBase mb in match)
            {
                ParameterInfo[] parameters = mb.GetParameters();

                // --- MODIFICA CHIAVE: Chiama la versione ParametersMatch per gli argomenti effettivi ---
                if (ParametersMatchForInvoke(parameters, args))
                {
                    return mb;
                }
            }
            return null;
        }

        public override object ChangeType(object value, Type type, CultureInfo culture)
        {
            try
            {
                // Gestisce la conversione per i tipi by-reference (ref/out)
                if (type.IsByRef)
                {
                    type = type.GetElementType(); // Ottiene il tipo sottostante per la conversione
                }
                object newType;
                newType = Convert.ChangeType(value, type, culture); // Usa anche culture per la conversione
                return newType;
            }
            catch (InvalidCastException)
            {
                return null; // O rilancia l'eccezione se preferisci un errore più esplicito
            }
        }

        public override void ReorderArgumentArray(ref object[] args, object state)
        {
            // Come avevi tu, non è necessaria alcuna riorganizzazione per ora.
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            foreach (MethodBase mb in match)
            {
                ParameterInfo[] parameters = mb.GetParameters();
                // --- MODIFICA CHIAVE: Chiama la versione ParametersMatch per i tipi formali ---
                if (ParametersMatchForGetMethod(parameters, types))
                {
                    return mb;
                }
            }
            return null;
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }
            foreach (PropertyInfo pi in match)
            {
                if (pi.PropertyType == returnType &&
                    ParametersMatchForGetMethod(pi.GetIndexParameters(), indexes)) // Usa il nuovo metodo
                {
                    return pi;
                }
            }
            return null;
        }

        /// <summary>
        /// Restituisce true solo se il tipo di ogni oggetto in 'a' corrisponde al tipo di ogni corrispondente oggetto in 'b' per l'INVOCAZIONE.
        /// Gestisce parametri 'out' e 'ref' e argomenti null.
        /// </summary>
        /// <param name="methodParameters">Parametri del metodo da confrontare.</param>
        /// <param name="actualArguments">Argomenti effettivi passati a Invoke.</param>
        /// <returns>True se i parametri corrispondono, altrimenti false.</returns>
        private bool ParametersMatchForInvoke(ParameterInfo[] methodParameters, object[] actualArguments)
        {
            if (methodParameters.Length != actualArguments.Length)
            {
                return false;
            }

            for (int i = 0; i < methodParameters.Length; i++)
            {
                Type parameterType = methodParameters[i].ParameterType;
                object argumentValue = actualArguments[i];

                if (parameterType.IsByRef) // È un parametro 'ref' o 'out'
                {
                    // Per 'out' e 'ref', il tipo del parametro è il tipo by-reference (es. string&).
                    // Dobbiamo confrontare il tipo sottostante (es. string) con il tipo dell'argomento.
                    Type elementType = parameterType.GetElementType();

                    // Se l'argomento passato è null (comune per 'out'), e il tipo sottostante non è un tipo valore non-nullable, è ok.
                    if (argumentValue == null)
                    {
                        if (elementType.IsValueType && Nullable.GetUnderlyingType(elementType) == null)
                        {
                            // Tipo valore non-nullable non può essere null
                            return false;
                        }
                    }
                    else // L'argomento non è null, verifica l'assegnabilità
                    {
                        if (!elementType.IsAssignableFrom(argumentValue.GetType()))
                        {
                            return false;
                        }
                    }
                }
                else // È un parametro pass-by-value
                {
                    if (argumentValue == null)
                    {
                        if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                        {
                            // Tipo valore non-nullable non può essere null
                            return false;
                        }
                    }
                    else if (!parameterType.IsAssignableFrom(argumentValue.GetType()))
                    {
                        // Il tipo dell'argomento non è assegnabile al tipo del parametro
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Restituisce true solo se il tipo di ogni parametro del metodo 'a' corrisponde al tipo formale di ogni corrispondente tipo in 'b' per GETMETHOD.
        /// Gestisce i tipi by-reference (es. typeof(T).MakeByRefType()).
        /// </summary>
        /// <param name="methodParameters">Parametri del metodo (da MethodInfo.GetParameters()).</param>
        /// <param name="formalTypes">Tipi formali forniti a GetMethod (es. new Type[] { typeof(int), typeof(string).MakeByRefType() }).</param>
        /// <returns>True se i tipi corrispondono, altrimenti false.</returns>
        private bool ParametersMatchForGetMethod(ParameterInfo[] methodParameters, Type[] formalTypes)
        {
            if (methodParameters.Length != formalTypes.Length)
            {
                return false;
            }

            for (int i = 0; i < methodParameters.Length; i++)
            {
                Type methodParamType = methodParameters[i].ParameterType; // Tipo del parametro del metodo (es. int, string&, List<T>)
                Type formalArgumentType = formalTypes[i]; // Tipo fornito a GetMethod (es. typeof(int), typeof(string).MakeByRefType())

                // Caso 1: Entrambi sono by-reference (out/ref)
                if (methodParamType.IsByRef && formalArgumentType.IsByRef)
                {
                    // I tipi sottostanti (senza '&') devono corrispondere
                    if (methodParamType.GetElementType() != formalArgumentType.GetElementType())
                    {
                        return false;
                    }
                }
                // Caso 2: Nessuno dei due è by-reference (pass-by-value)
                else if (!methodParamType.IsByRef && !formalArgumentType.IsByRef)
                {
                    if (methodParamType != formalArgumentType)
                    {
                        return false;
                    }
                }
                // Caso 3: Mismatch by-reference (uno è by-ref, l'altro no)
                else
                {
                    return false; // Non possono essere match
                }
            }
            return true;
        }
    }
}
