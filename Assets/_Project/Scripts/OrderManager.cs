using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mode3D.Destinations
{
	/// <summary>
	/// Gestionnaire de commandes - sauvegarde et charge les commandes
	/// </summary>
	public static class OrderManager
	{
		private const string OrderCountKey = "Mode3D_OrderCount";
		private const string OrderPrefix = "Mode3D_Order_";

		[Serializable]
		public class OrderData
		{
			public string orderNumber;
			public string customerName;
			public string deliveryAddress;
			public float totalPrice;
			public List<OrderItem> items = new List<OrderItem>();
			public string destination;
			public string orderDate;
		}

		[Serializable]
		public class OrderItem
		{
			public string category;
			public string material;
			public int dayNumber;
			public float price;
		}

		/// <summary>
		/// Génère un numéro de commande aléatoire
		/// </summary>
		public static string GenerateOrderNumber()
		{
			return "CMD-" + UnityEngine.Random.Range(10000, 99999).ToString();
		}

		/// <summary>
		/// Sauvegarde une commande complète (avec nom et adresse)
		/// </summary>
		public static void SaveOrder(string orderNumber, string customerName, string deliveryAddress, 
									  List<OutfitProposalUI.OutfitPresentation> outfits, float totalPrice, string destination)
		{
			// Convertir les outfits en OrderItems
			List<OrderItem> orderItems = new List<OrderItem>();
			foreach (var outfit in outfits)
			{
				orderItems.Add(new OrderItem
				{
					category = outfit.category.ToString(),
					material = outfit.selectedMaterial,
					dayNumber = outfit.dayNumber,
					price = GetPriceForCategory(outfit.category)
				});
			}

			// Obtenir le nombre actuel de commandes
			int orderCount = PlayerPrefs.GetInt(OrderCountKey, 0);
			string orderKey = OrderPrefix + orderCount;

			// Sauvegarder la commande
			PlayerPrefs.SetString(orderKey + "_Number", orderNumber);
			PlayerPrefs.SetString(orderKey + "_Name", customerName);
			PlayerPrefs.SetString(orderKey + "_Address", deliveryAddress);
			PlayerPrefs.SetFloat(orderKey + "_Price", totalPrice);
			PlayerPrefs.SetString(orderKey + "_Destination", destination);
			PlayerPrefs.SetString(orderKey + "_Date", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
			PlayerPrefs.SetInt(orderKey + "_ItemCount", orderItems.Count);

			for (int i = 0; i < orderItems.Count; i++)
			{
				PlayerPrefs.SetString(orderKey + "_Item_" + i + "_Category", orderItems[i].category);
				PlayerPrefs.SetString(orderKey + "_Item_" + i + "_Material", orderItems[i].material);
				PlayerPrefs.SetInt(orderKey + "_Item_" + i + "_Day", orderItems[i].dayNumber);
				PlayerPrefs.SetFloat(orderKey + "_Item_" + i + "_Price", orderItems[i].price);
			}

			// Incrémenter le compteur
			PlayerPrefs.SetInt(OrderCountKey, orderCount + 1);
			PlayerPrefs.Save();
		}

		/// <summary>
		/// Charge toutes les commandes
		/// </summary>
		public static List<OrderData> LoadAllOrders()
		{
			List<OrderData> orders = new List<OrderData>();
			int orderCount = PlayerPrefs.GetInt(OrderCountKey, 0);

			for (int orderIndex = 0; orderIndex < orderCount; orderIndex++)
			{
				string orderKey = OrderPrefix + orderIndex;
				
				OrderData order = new OrderData
				{
					orderNumber = PlayerPrefs.GetString(orderKey + "_Number", ""),
					customerName = PlayerPrefs.GetString(orderKey + "_Name", ""),
					deliveryAddress = PlayerPrefs.GetString(orderKey + "_Address", ""),
					totalPrice = PlayerPrefs.GetFloat(orderKey + "_Price", 0f),
					destination = PlayerPrefs.GetString(orderKey + "_Destination", ""),
					orderDate = PlayerPrefs.GetString(orderKey + "_Date", "")
				};

				int itemCount = PlayerPrefs.GetInt(orderKey + "_ItemCount", 0);
				for (int i = 0; i < itemCount; i++)
				{
					order.items.Add(new OrderItem
					{
						category = PlayerPrefs.GetString(orderKey + "_Item_" + i + "_Category", ""),
						material = PlayerPrefs.GetString(orderKey + "_Item_" + i + "_Material", ""),
						dayNumber = PlayerPrefs.GetInt(orderKey + "_Item_" + i + "_Day", 0),
						price = PlayerPrefs.GetFloat(orderKey + "_Item_" + i + "_Price", 0f)
					});
				}

				orders.Add(order);
			}

			return orders;
		}

		/// <summary>
		/// Vérifie si des commandes existent
		/// </summary>
		public static bool HasOrder()
		{
			return PlayerPrefs.GetInt(OrderCountKey, 0) > 0;
		}

		private static float GetPriceForCategory(OutfitType category)
		{
			switch (category)
			{
				case OutfitType.Chill: return 45.99f;
				case OutfitType.Sport: return 65.99f;
				case OutfitType.Business: return 120.00f;
				default: return 50.00f;
			}
		}
	}
}

