using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using EPiServer;
using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Geta.Commerce.Payments.PayPal.Extensions;
using Geta.PayPal;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Catalog.Managers;
using Mediachase.Commerce.Catalog.Objects;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Dto;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Commerce.Website;
using Mediachase.Commerce.Website.Helpers;
using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;

namespace Geta.Commerce.Payments.PayPal.Extensions
{
    public static class ILineItemExtensions
    {
         /// <summary>
        /// Get display name with current language
        /// </summary>
        /// <param name="item">The line item of oder</param>
        /// <param name="maxSize">The number of character to get display name</param>
        /// <returns>Display name with current language</returns>
        public static string GetDisplayNameOfCurrentLanguage(this ILineItem item, int maxSize)
        {
            Entry entry = CatalogContext.Current.GetCatalogEntry(item.Code, new CatalogEntryResponseGroup(CatalogEntryResponseGroup.ResponseGroup.CatalogEntryFull));
            // if the entry is null (product is deleted), return item display name
            var displayName = (entry != null) ? StoreHelper.GetEntryDisplayName(entry).StripPreviewText(maxSize <= 0 ? 100 : maxSize) : item.DisplayName.StripPreviewText(maxSize <= 0 ? 100 : maxSize);

            return displayName;
        }

    }
}