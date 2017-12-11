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

namespace UniEBoard.HtmlControls
{
    /// <summary>
    /// Create an HTML week view List from a resursive collection of items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WeekView<T> : IHtmlString
    {
        private readonly HtmlHelper _html;
        private readonly IEnumerable<T> _items = Enumerable.Empty<T>();
        private Func<T, string> _displayProperty = item => item.ToString();
        private string _headerHTML;
        private string _footerHTML;
        private string _itemSeperatorHTML;
        private Func<T, HelperResult> _ItemTemplate;
        private Func<T, HelperResult> _emptyTemplate;
        private RouteValueDictionary _htmlAttributes = new RouteValueDictionary();
        private RouteValueDictionary _altHtmlAttributes = new RouteValueDictionary();
        private Func<T, int> _dayOfTheWeek;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeekView&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="items">The items.</param>
        /// <param name="DatePropertyIdentifier">The date property identifier.</param>
        public WeekView(HtmlHelper html, IEnumerable<T> items, string datePropertyIdentifier)
        {
            if (html == null) throw new ArgumentNullException("No HtmlHelper defined");
            if (string.IsNullOrEmpty(datePropertyIdentifier)) throw new ArgumentNullException("No Date Property Identifier defined");
            _html = html;
            _items = items;
            _dayOfTheWeek = item => (int)GetDayOfWeekfromProperty(item, datePropertyIdentifier);
        }

        /// <summary>
        /// Getfroms the property.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public DayOfWeek GetDayOfWeekfromProperty<TItem>(TItem item, string property)
        {
            DayOfWeek dayOftheWeek = DayOfWeek.Sunday;
            try
            {
                Type propertytype = typeof(TItem);
                PropertyInfo propertyInfo = propertytype.GetProperty(property);
                object value = propertyInfo.GetValue(item, null);
                if (value is DateTime)
                {
                    dayOftheWeek = ((DateTime)value).DayOfWeek;
                }
            }
            catch (Exception)
            {
            }
            return dayOftheWeek;
        }

        /// <summary>
        /// Headers the HTML.
        /// </summary>
        /// <param name="headerHTML">The header HTML.</param>
        /// <returns></returns>
        public WeekView<T> HeaderHTML(string headerHTML)
        {
            _headerHTML = headerHTML;
            return this;
        }

        /// <summary>
        /// Footers the HTML.
        /// </summary>
        /// <param name="footerHTML">The footer HTML.</param>
        /// <returns></returns>
        public WeekView<T> FooterHTML(string footerHTML)
        {
            _footerHTML = footerHTML;
            return this;
        }

        /// <summary>
        /// Items the seperator HTML.
        /// </summary>
        /// <param name="itemSeperatorHTML">The item seperator HTML.</param>
        /// <returns></returns>
        public WeekView<T> ItemSeperatorHTML(string itemSeperatorHTML)
        {
            _itemSeperatorHTML = itemSeperatorHTML;
            return this;
        }

        /// <summary>
        /// Items the template.
        /// </summary>
        /// <param name="itemTemplate">The item template.</param>
        /// <returns></returns>
        public WeekView<T> ItemTemplate(Func<T, HelperResult> itemTemplate)
        {
            if (itemTemplate == null) throw new ArgumentNullException("No itemTemplate defined");
            _ItemTemplate = itemTemplate;
            return this;
        }

        /// <summary>
        /// Empties the template.
        /// </summary>
        /// <param name="emptyTemplate">The empty template.</param>
        /// <returns></returns>
        public WeekView<T> EmptyTemplate(Func<T, HelperResult> emptyTemplate)
        {
            if (emptyTemplate == null) throw new ArgumentNullException("No emptyTemplate defined");
            _emptyTemplate = emptyTemplate;
            return this;
        }

        /// <summary>
        /// HTMLs the attributes.
        /// </summary>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns></returns>
        public WeekView<T> HtmlAttributes(object htmlAttributes)
        {
            if (htmlAttributes != null)
            {
                _htmlAttributes = new RouteValueDictionary(htmlAttributes);
            }
            return this;
        }

        /// <summary>
        /// Alts the HTML attributes.
        /// </summary>
        /// <param name="altHtmlAttributes">The alt HTML attributes.</param>
        /// <returns></returns>
        public WeekView<T> AltHtmlAttributes(object altHtmlAttributes)
        {
            if (altHtmlAttributes != null)
            {
                _altHtmlAttributes = new RouteValueDictionary(altHtmlAttributes);
            }
            return this;
        }

        /// <summary>
        /// Returns an HTML-encoded string.
        /// </summary>
        /// <returns>An HTML-encoded string.</returns>
        public string ToHtmlString()
        {
            return ToString();
        }

        /// <summary>
        /// Renders this instance.
        /// </summary>
        public void Render()
        {
            var writer = _html.ViewContext.Writer;
            using (var textWriter = new HtmlTextWriter(writer))
            {
                textWriter.Write(ToString());
            }
        }

        private void ValidateSettings()
        {
            if (_items == null)
            {
                throw new InvalidOperationException("You must specify an IEnumerable Collection of Items to build the WeekView From");
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            ValidateSettings();
            var listItems = _items.ToList();
            StringBuilder weekBuilder = new StringBuilder();
            bool useAltAttributes = false;
            for (int dayOfWeek = 1; dayOfWeek <= 5; dayOfWeek++)
            {
                // Build Day Container
                var dayHeader = new TagBuilder("h6");
                dayHeader.InnerHtml = ((DayOfWeek)dayOfWeek).ToString();
                var dayContainer = new TagBuilder("div");
                dayContainer.InnerHtml += dayHeader.ToString(TagRenderMode.Normal);
                dayContainer.MergeAttributes(useAltAttributes ? _altHtmlAttributes : _htmlAttributes);
                useAltAttributes = !useAltAttributes;

                // Build Day Items       
                string dayItems = GetDayItems(dayOfWeek, listItems);
                dayContainer.InnerHtml += _headerHTML;
                dayContainer.InnerHtml += !string.IsNullOrEmpty(dayItems) ? dayItems : GetEmptyTemplate();
                dayContainer.InnerHtml += _footerHTML;

                weekBuilder.Append(dayContainer.ToString(TagRenderMode.Normal));
            }
            return weekBuilder.ToString();
        }

        /// <summary>
        /// Gets the day items.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="allItems">All items.</param>
        /// <returns></returns>
        private string GetDayItems(int day, List<T> allItems)
        {
            StringBuilder dayItems = new StringBuilder();
            bool isFirstItem = true;
            foreach (var item in allItems)
            {
                if (_dayOfTheWeek(item) == day)
                {
                    if (!isFirstItem)
                    {
                        dayItems.Append(_itemSeperatorHTML);
                    }

                    dayItems.Append(GetItem(item));
                    isFirstItem = false;
                }
            }
            return dayItems.ToString();
        }

        /// <summary>
        /// Gets the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private string GetItem(T item)
        {
            return _ItemTemplate(item).ToHtmlString();
        }

        /// <summary>
        /// Gets the empty template.
        /// </summary>
        /// <returns></returns>
        private string GetEmptyTemplate()
        {
            return _emptyTemplate(default(T)).ToHtmlString();
        }

    }
}