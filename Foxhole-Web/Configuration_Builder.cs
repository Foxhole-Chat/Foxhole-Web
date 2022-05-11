using Foxhole_Web.Models;
using System.Reflection;
using System.Security.Cryptography;
using Tommy;

namespace Foxhole_Web
{
	public static class Config_Builder
	{
		private static string Encrypt_String(string value)
		{
			byte[] encrypted_value = Array.Empty<byte>();
			byte[] key = new byte[32];
			byte[] iv = new byte[16];
			Random random = new();

			random.NextBytes(key);
			random.NextBytes(iv);


			using (StreamWriter file = new("Encryption_Key.txt"))
			{
				file.WriteLine(Convert.ToBase64String(key));
			}

			using (StreamWriter file = new("Encryption_IV.txt"))
			{
				file.WriteLine(Convert.ToBase64String(iv));
			}


			Aes aes = Aes.Create();

			using (MemoryStream memory_stream = new())
			{
				using (CryptoStream crypto_stream = new(memory_stream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
				{
					using StreamWriter swEncrypt = new(crypto_stream);

					swEncrypt.Write(value);
				}

				encrypted_value = memory_stream.ToArray();
			}

			return Convert.ToBase64String(encrypted_value);
		}

		private static string Decrypt_String(string value)
		{
			byte[] encrypted_value = Convert.FromBase64String(value);

			byte[] key = new byte[32];
			byte[] iv = new byte[16];


			using (StreamReader file = new("Encryption_Key.txt"))
			{
				key = Convert.FromBase64String(file.ReadToEnd());
			}

			using (StreamReader file = new("Encryption_IV.txt"))
			{
				iv = Convert.FromBase64String(file.ReadToEnd());
			}


			Aes aes = Aes.Create();

			using MemoryStream memory_stream = new(encrypted_value);

			using CryptoStream crypto_stream = new(memory_stream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);

			return new StreamReader(crypto_stream).ReadToEnd();
		}



		private static TomlTable Generate_New_Config(string file)
		{
			Console.WriteLine("\nWould you like to create a new configuration file? (Y/N)");

			string response = Console.ReadLine()?.ToLower() ?? string.Empty;

			if (response != string.Empty &&
				 response[0] != 'y') { Environment.Exit(-1); }


			Type config_type = typeof(Config);
			Type[] nested_types = config_type.GetNestedTypes(),
				config_types = new Type[1 + nested_types.Length];

			config_types[0] = config_type;
			nested_types.CopyTo(config_types, 1);

			TomlTable new_config = new();

			foreach (Type type in config_types)
			{
				PropertyInfo[] properties = type.GetProperties();

				foreach (PropertyInfo property in properties)
				{
					Type property_type = property.PropertyType;

					if (property_type == typeof(string))
					{
						Console.WriteLine("\nPlease set property: \"" + property.Name + "\"");

						string? default_value = property.GetValue(property)?.ToString();
						bool has_default_value = default_value != null && default_value != string.Empty;


						if (has_default_value) { Console.WriteLine("Default value: \"" + default_value + '\"'); }

						string value;

						do { value = Console.ReadLine() ?? string.Empty; }
						while (!has_default_value && value == string.Empty);

						if (property.Name.Length >= 9 && property.Name.Substring(property.Name.Length - 9, 9) == "_Password") { value = Encrypt_String(value); }

						if (type.Name == config_type.Name) { new_config[property.Name] = value == "" ? default_value : value; }
						else { new_config[type.Name][property.Name] = value == "" ? default_value : value; }
					}
				}
			}

			using StreamWriter writer = File.CreateText(file);
			new_config.WriteTo(writer);

			return new_config;
		}

		public static void Read_Config_File(string file)
		{
			TomlTable config;

			try
			{
				using StreamReader reader = File.OpenText(file);
				config = TOML.Parse(reader);
				reader.Close();
			}
			catch (Exception exception) when (exception is FileNotFoundException || exception is DirectoryNotFoundException)
			{
				Console.WriteLine("The file \"" + file + "\" was not found...");
				config = Generate_New_Config(file);
			}


			Type config_type = typeof(Config);
			Type[] nested_types = config_type.GetNestedTypes(),
				config_types = new Type[1 + nested_types.Length];

			config_types[0] = config_type;
			nested_types.CopyTo(config_types, 1);

			while (true)
			{
				try
				{
					foreach (Type type in config_types)
					{
						PropertyInfo[] properties = type.GetProperties();

						foreach (PropertyInfo property in properties)
						{
							Type property_type = property.PropertyType;
							if (property_type == typeof(string))
							{
								string property_value = string.Empty;

								if (type.Name == config_type.Name) { property_value = config[property.Name].ToString() ?? string.Empty; }
								else { property_value = config[type.Name][property.Name].ToString() ?? string.Empty; }

								if (property_value == "Tommy.TomlLazy") { throw new MissingFieldException("Missing configuration parameter: \"" + property.Name + "\"..."); }

								if (property.Name.Length > 9 && property.Name.Substring(property.Name.Length - 9, 9) == "_Password") { property_value = Decrypt_String(property_value); }

								property.SetValue(property, property_value);
							}
						}
					}
					break;
				}
				catch (MissingFieldException exception)
				{
					Console.WriteLine(exception.Message);
					config = Generate_New_Config(file);
				}
			}
		}
	}
}
