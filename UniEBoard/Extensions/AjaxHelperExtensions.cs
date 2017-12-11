// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlHelperExtensions.cs" company="Cognite Ltd">
//   Copyright (c) 2013 Cognite Ltd (http://www.cognite.co.uk)
// </copyright>
// <summary>
//  HtmlHelper Extension Methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.WebPages;
using System.Text;
using System.Web.Routing;
using System.Linq.Expressions;
using System.Reflection;
using UniEBoard.HtmlControls;

namespace System.Web.Mvc.Html
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Create an HTML tree from a recursive collection of items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="html">The HTML.</param>
        /// <param name="items">The items.</param>
        /// <param name="datePropertyIdentifier">The date property identifier to sort items by.</param>
        /// <returns></returns>
        public static WeekView<T> WeekView<T>(this HtmlHelper html, IEnumerable<T> items, string datePropertyIdentifier)
        {
            return new WeekView<T>(html, items, datePropertyIdentifier);
        }

        public static MvcHtmlString CustomDropDownList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> selectList, string selectedValue, string optionLabel, object htmlAttributes)
        {
            int? selectedItem = null;
            if (!string.IsNullOrEmpty(selectedValue)) selectedItem = int.Parse(selectedValue);
            return (CustomDropDownList(htmlHelper, name, selectList, selectedItem, optionLabel, htmlAttributes));
        }
        
        /// <summary>
        /// Simples the text area for.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="name">The name.</param>
        /// <param name="selectList">The select list.</param>
        /// <param name="optionLabel">The option label.</param>
        /// <param name="selectedValue">The selected value.</param>
        /// <returns></returns>
        public static MvcHtmlString CustomDropDownList(this HtmlHelper htmlHelper, string name, IEnumerable<SelectListItem> selectList, int? selectedValue, string optionLabel, object htmlAttributes)
        {
            if (selectList != null)
            {
                foreach (var item in selectList)
                {
                    item.Selected = false;
                    if (selectedValue.HasValue && item.Value.Equals(selectedValue.Value.ToString()))
                    {
                        if (selectedValue.Value >= 0)
                        {
                            item.Selected = true;
                            //break;
                        }
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }
            }
            return !string.IsNullOrEmpty(optionLabel)
                ? htmlHelper.DropDownList(name, selectList, optionLabel, htmlAttributes)
                : htmlHelper.DropDownList(name, selectList, htmlAttributes);
        }
    }
}
