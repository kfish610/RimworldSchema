using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using UnityEngine;
using Verse;

namespace RimworldSchema
{
    class Program
    {
        public static readonly Dictionary<Type, string> Aliases =
        new Dictionary<Type, string>()
        {
                        [typeof(byte)] = "unsignedByte",
                        [typeof(sbyte)] = "byte",
                        [typeof(short)] = "short",
                        [typeof(ushort)] = "unsignedShort",
                        [typeof(int)] = "int",
                        [typeof(uint)] = "unsignedInt",
                        [typeof(long)] = "long",
                        [typeof(ulong)] = "unsignedLong",
                        [typeof(float)] = "float",
                        [typeof(double)] = "double",
                        [typeof(decimal)] = "decimal",
                        [typeof(bool)] = "boolean",
                        [typeof(char)] = "string",
                        [typeof(string)] = "string",
                        [typeof(Type)] = "string",
                        [typeof(BuildableDef)] = "string"
        };

        public static List<XmlSchemaType> types = new List<XmlSchemaType>();

        static void Main(string[] args)
        {
            Assembly rimworld = Assembly.Load("Assembly-CSharp, Version=1.0.6955.24033, Culture=neutral, PublicKeyToken=null");

            var defs =
                from type in rimworld.GetExportedTypes()
                where type.IsClass && !type.IsAbstract && Regex.Match(type.Name, @"\wDef$").Success
                select type;

            XmlSchema schema = new XmlSchema
            {
                TargetNamespace = "https://github.com/kfish610/RimWorldSchema"
            };

            var defsRoot = new XmlSchemaElement
            {
                Name = "Defs"
            };

            var defsType = new XmlSchemaComplexType();
            defsRoot.SchemaType = defsType;

            var defsSeq = new XmlSchemaSequence();
            defsType.Particle = defsSeq;

            schema.Items.Add(defsRoot);

            foreach (var def in defs)
            {
                Aliases.Add(def, "string");
            }

            foreach (var def in defs)
            {
                defsSeq.Items.Add(new XmlSchemaElement()
                {
                    Name = def.Name,
                    SchemaType = Derive(def),
                    MinOccurs = 0
                });
            }

            foreach (var type in types)
            {
                schema.Items.Insert(0, type);
            }

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += new ValidationEventHandler((_, e) => Console.WriteLine(e.Message));
            schemaSet.Add(schema);
            schemaSet.Compile();

            XmlSchema compiledSchema = null;

            foreach (XmlSchema schema1 in schemaSet.Schemas())
            {
                compiledSchema = schema1;
            }

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace(string.Empty, "https://github.com/kfish610/RimWorldSchema");
            nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");

            File.WriteAllText(@"schema.xsd", string.Empty);
            using StreamWriter file = new StreamWriter(@"schema.xsd");
            compiledSchema.Write(file, nsmgr);
        }

        private static readonly List<string> definedTypes = new List<string>();

        public static XmlSchemaType Derive(Type type)
        {
            if (type.BaseType == typeof(Enum))
            {
                var restriction = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                foreach (var name in Enum.GetNames(type))
                {
                    restriction.Facets.Add(new XmlSchemaEnumerationFacet()
                    {
                        Value = name
                    });
                }
                return new XmlSchemaSimpleType()
                {
                    Content = restriction
                };
            }
            if (type == typeof(IntVec2))
            {
                var restriction = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                restriction.Facets.Add(new XmlSchemaPatternFacet()
                {
                    Value = @"\(([0-9])+, ?([0-9])+\)"
                });
                return new XmlSchemaSimpleType()
                {
                    Content = restriction
                };
            }
            if (type == typeof(IntVec3))
            {
                var restriction = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                restriction.Facets.Add(new XmlSchemaPatternFacet()
                {
                    Value = @"\(([0-9])+, ?([0-9])+, ?([0-9])+\)"
                });
                return new XmlSchemaSimpleType()
                {
                    Content = restriction
                };
            }
            if (type == typeof(Vector2))
            {
                var restriction = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                restriction.Facets.Add(new XmlSchemaPatternFacet()
                {
                    Value = @"\(([0-9])+(\.([0-9])+)?, ?([0-9])+(\.([0-9])+)?\)"
                });
                return new XmlSchemaSimpleType()
                {
                    Content = restriction
                };
            }
            if (type == typeof(Vector3))
            {
                var restriction = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                restriction.Facets.Add(new XmlSchemaPatternFacet()
                {
                    Value = @"\(([0-9])+(\.([0-9])+)?, ?([0-9])+(\.([0-9])+)?, ?([0-9])+(\.([0-9])+)?\)"
                });
                return new XmlSchemaSimpleType()
                {
                    Content = restriction
                };
            }
            if (type == typeof(Color))
            {
                var restriction = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
                };
                restriction.Facets.Add(new XmlSchemaPatternFacet()
                {
                    Value = @"\(([0-9])+(\.([0-9])+)?, ?([0-9])+(\.([0-9])+)?, ?([0-9])+(\.([0-9])+)?(, ?([0-9])+(\.([0-9])+)?)?\)"
                });
                return new XmlSchemaSimpleType()
                {
                    Content = restriction
                };
            }
            var fields = type.GetFields();
            var seq = new XmlSchemaSequence();
            foreach (var field in fields)
            {
                if (field.FieldType.GetInterfaces().Contains(typeof(IExposable)))
                    continue;
                object attr = Attribute.GetCustomAttribute(field, typeof(UnsavedAttribute));
                if (attr != null)
                    continue;
                if (Aliases.ContainsKey(field.FieldType))
                {
                    seq.Items.Add(new XmlSchemaElement()
                    {
                        Name = field.Name,
                        SchemaTypeName = new XmlQualifiedName(Aliases[field.FieldType], "http://www.w3.org/2001/XMLSchema"),
                        MinOccurs = 0
                    });
                }
                else
                {
                    if (field.FieldType.IsGenericType && field.FieldType.GetGenericArguments().Any(x => x.IsGenericType)) { }
                    else if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>) || field.FieldType.GetGenericTypeDefinition() == typeof(HashSet<>)))
                    {
                        Type elemT = field.FieldType.GetGenericArguments()[0];
                        if (!Aliases.ContainsKey(elemT) && !definedTypes.Any(x => x == elemT.Name.ToCamelCase()))
                        {
                            definedTypes.Add(elemT.Name.ToCamelCase());
                            var t = Derive(elemT);
                            t.Name = elemT.Name.ToCamelCase();
                            types.Add(t);
                        }
                        var list = new XmlSchemaSequence();
                        list.Items.Add(new XmlSchemaElement()
                        {
                            Name = "li",
                            SchemaTypeName = Aliases.ContainsKey(elemT) ? new XmlQualifiedName(Aliases[elemT], "http://www.w3.org/2001/XMLSchema") : new XmlQualifiedName(elemT.Name.ToCamelCase(), "https://github.com/kfish610/RimWorldSchema"),
                            MinOccurs = 0,
                            MaxOccursString = "unbounded"
                        });
                        seq.Items.Add(new XmlSchemaElement()
                        {
                            Name = field.Name,
                            SchemaType = new XmlSchemaComplexType()
                            {
                                Particle = list
                            },
                            MinOccurs = 0
                        });
                    }
                    else if (field.FieldType.IsArray)
                    {
                        Type elemT = field.FieldType.GetElementType();
                        if (!Aliases.ContainsKey(elemT) && !definedTypes.Any(x => x == elemT.Name.ToCamelCase()))
                        {
                            definedTypes.Add(elemT.Name.ToCamelCase());
                            var t = Derive(elemT);
                            t.Name = elemT.Name.ToCamelCase();
                            types.Add(t);
                        }
                        var list = new XmlSchemaSequence();
                        list.Items.Add(new XmlSchemaElement()
                        {
                            Name = "li",
                            SchemaTypeName = Aliases.ContainsKey(elemT) ? new XmlQualifiedName(Aliases[elemT], "http://www.w3.org/2001/XMLSchema") : new XmlQualifiedName(elemT.Name.ToCamelCase(), "https://github.com/kfish610/RimWorldSchema"),
                            MinOccurs = 0,
                            MaxOccursString = "unbounded"
                        });
                        seq.Items.Add(new XmlSchemaElement()
                        {
                            Name = field.Name,
                            SchemaType = new XmlSchemaComplexType()
                            {
                                Particle = list
                            },
                            MinOccurs = 0
                        });
                    }
                    else if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
                    {
                        Type keyT = field.FieldType.GetGenericArguments()[0];
                        Type valueT = field.FieldType.GetGenericArguments()[1];
                        if (!Aliases.ContainsKey(keyT) && !definedTypes.Any(x => x == keyT.Name.ToCamelCase()))
                        {
                            definedTypes.Add(keyT.Name.ToCamelCase());
                            var t = Derive(keyT);
                            t.Name = keyT.Name.ToCamelCase();
                            types.Add(t);
                        }
                        if (!Aliases.ContainsKey(valueT) && !definedTypes.Any(x => x == valueT.Name.ToCamelCase()))
                        {
                            definedTypes.Add(valueT.Name.ToCamelCase());
                            var t = Derive(valueT);
                            t.Name = valueT.Name.ToCamelCase();
                            types.Add(t);
                        }
                        var kv = new XmlSchemaSequence();
                        kv.Items.Add(new XmlSchemaElement()
                        {
                            Name = "key",
                            SchemaTypeName = Aliases.ContainsKey(keyT) ? new XmlQualifiedName(Aliases[keyT], "http://www.w3.org/2001/XMLSchema") : new XmlQualifiedName(keyT.Name.ToCamelCase(), "https://github.com/kfish610/RimWorldSchema")
                        });
                        kv.Items.Add(new XmlSchemaElement()
                        {
                            Name = "value",
                            SchemaTypeName = Aliases.ContainsKey(valueT) ? new XmlQualifiedName(Aliases[valueT], "http://www.w3.org/2001/XMLSchema") : new XmlQualifiedName(valueT.Name.ToCamelCase(), "https://github.com/kfish610/RimWorldSchema")
                        });
                        var dict = new XmlSchemaSequence();
                        dict.Items.Add(new XmlSchemaElement()
                        {
                            Name = "li",
                            SchemaType = new XmlSchemaComplexType()
                            {
                                Particle = kv
                            },
                            MinOccurs = 0,
                            MaxOccursString = "unbounded"
                        });
                        seq.Items.Add(new XmlSchemaElement()
                        {
                            Name = field.Name,
                            SchemaType = new XmlSchemaComplexType()
                            {
                                Particle = dict
                            },
                            MinOccurs = 0
                        });
                    }
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var nullableT = field.FieldType.GetGenericArguments()[0];
                        if (!Aliases.ContainsKey(nullableT) && !definedTypes.Any(x => x == nullableT.Name.ToCamelCase()))
                        {
                            definedTypes.Add(nullableT.Name.ToCamelCase());
                            var t = Derive(nullableT);
                            t.Name = nullableT.Name.ToCamelCase();
                            types.Add(t);
                        }
                        seq.Items.Add(new XmlSchemaElement()
                        {
                            Name = field.Name,
                            SchemaTypeName = Aliases.ContainsKey(nullableT) ? new XmlQualifiedName(Aliases[nullableT], "http://www.w3.org/2001/XMLSchema") : new XmlQualifiedName(nullableT.Name.ToCamelCase(), "https://github.com/kfish610/RimWorldSchema"),
                            MinOccurs = 0
                        });
                    }
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Predicate<>)) { }
                    else if (field.FieldType.IsGenericType)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(field.FieldType.GetGenericTypeDefinition().Name + " in " + type.Name + " is an Unknown Generic");
                        Console.ReadKey();
                        Console.ResetColor();
                    }
                    else
                    {
                        if (!definedTypes.Any(x => x == field.FieldType.Name.ToCamelCase()))
                        {
                            definedTypes.Add(field.FieldType.Name.ToCamelCase());
                            var t = Derive(field.FieldType);
                            t.Name = field.FieldType.Name.ToCamelCase();
                            types.Add(t);
                        }
                        seq.Items.Add(new XmlSchemaElement()
                        {
                            Name = field.Name,
                            SchemaTypeName = new XmlQualifiedName(field.FieldType.Name.ToCamelCase(), "https://github.com/kfish610/RimWorldSchema"),
                            MinOccurs = 0
                        });
                    }
                }
            }
            return new XmlSchemaComplexType()
            {
                Particle = seq
            };
        }
    }
}
