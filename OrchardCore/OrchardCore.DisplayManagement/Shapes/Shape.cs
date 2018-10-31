using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.UI;

namespace OrchardCore.DisplayManagement.Shapes
{
	[DebuggerTypeProxy(typeof(ShapeDebugView))]
	public class Shape : Composite, IShape, IPositioned, IEnumerable<object>
	{
		private List<string> _classes;
		private Dictionary<string, string> _attributes;
		private readonly List<IPositioned> _items = new List<IPositioned>();
		private bool _sorted = false;

        public ShapeMetadata Metadata { get; } = new ShapeMetadata();

        public string Id { get; set; }
		public IList<string> Classes => _classes = _classes ?? new List<string>();
		public IDictionary<string, string> Attributes => _attributes = _attributes ?? new Dictionary<string, string>();
		public IEnumerable<dynamic> Items => _items;
		public bool HasItems => _items.Count > 0;
		public string Position
		{
			get { return Metadata.Position; }
			set { Metadata.Position = value; }
		}

		public virtual Shape Add(object item, string position = null)
		{
			if (item == null)
			{
				return this;
			}

			if (position == null)
			{
				position = "";
			}

			_sorted = false;

			if (item is IHtmlContent)
			{
				_items.Add(new PositionWrapper((IHtmlContent)item, position));
			}
			else if (item is string)
			{
				_items.Add(new PositionWrapper((string)item, position));
			}
			else
			{
				var shape = item as IPositioned;
				if (shape != null)
				{
					if (position != null)
					{
						shape.Position = position;
					}

					_items.Add(shape);
				}
			}

			return this;
		}

		public Shape AddRange(IEnumerable<object> items, string position = null)
		{
			foreach (var item in items)
			{
				Add(item, position);
			}

			return this;
		}

		public void Remove(string shapeName)
		{
			for (var i = _items.Count - 1; i >= 0 ; i--)
			{
                if (_items[i] is IShape shape && shape.Metadata.Name == shapeName)
                {
                    _items.RemoveAt(i);
					return;
				}
			}
		}

		public IShape Named(string shapeName)
		{
			for (var i = 0; i < _items.Count; i++)
			{
                if (_items[i] is IShape shape && shape.Metadata.Name == shapeName)
				{
					return shape;
				}
			}

			return null;
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			if (!_sorted)
			{
				_items.Sort(FlatPositionComparer.Instance);
				_sorted = true;
			}

			return _items.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			if (!_sorted)
			{
				_items.Sort(FlatPositionComparer.Instance);
				_sorted = true;
			}

			return _items.GetEnumerator();
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			result = Items;

			if (binder.ReturnType == typeof(IEnumerable<object>) ||
				binder.ReturnType == typeof(IEnumerable<dynamic>))
			{
				return true;
			}

			return base.TryConvert(binder, out result);
		}

		public static TagBuilder GetTagBuilder(dynamic shape, string defaultTag = "span")
		{
			string tagName = shape.Tag;

			// Dont replace by ?? as shape.Tag is dynamic
			if (tagName == null)
			{
				tagName = defaultTag;
			}

			string id = shape.Id;
			IEnumerable<string> classes = shape.Classes;
			IDictionary<string, string> attributes = shape.Attributes;

			return GetTagBuilder(tagName, id, classes, attributes);
		}

		public static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes)
		{
			var tagBuilder = new TagBuilder(tagName);

			if (attributes != null)
			{
				tagBuilder.MergeAttributes(attributes, false);
			}

			foreach (var cssClass in classes ?? Enumerable.Empty<string>())
			{
				tagBuilder.AddCssClass(cssClass);
			}

			if (!string.IsNullOrWhiteSpace(id))
			{
				tagBuilder.Attributes["id"] = id;
			}
			return tagBuilder;
		}
	}
}