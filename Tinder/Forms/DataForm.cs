// Copyright (C) 2004-2009 Jive Software. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using org.xmpp.packet;
using org.xmpp.util;
using System;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace org.xmpp.forms
{
	/**
 * Represents a form that could be use for gathering data as well as for reporting data
 * returned from a search.
 * <p/>
 * The form could be of the following types:
 * <ul>
 * <li>form -> Indicates a form to fill out.</li>
 * <li>submit -> The form is filled out, and this is the data that is being returned from
 * the form.</li>
 * <li>cancel -> The form was cancelled. Tell the asker that piece of information.</li>
 * <li>result -> Data results being returned from a search, or some other query.</li>
 * </ul>
 * <p/>
 * In case the form represents a search, the report will be structured in columns and rows. Use
 * {@link #addReportedField(String,String,FormField.Type)} to set the columns of the report whilst
 * the report's rows can be configured using {@link #addItemFields(Map)}.
 *
 * @author Gaston Dombiak
 */
	public class DataForm : PacketExtension
	{
		private static readonly SimpleDateFormat UTC_FORMAT = new SimpleDateFormat(
			XMPPConstants.XMPP_DELAY_DATETIME_FORMAT);
		private static readonly FastDateFormat FAST_UTC_FORMAT =
			FastDateFormat.getInstance(XMPPConstants.XMPP_DELAY_DATETIME_FORMAT,
			                           TimeZone.getTimeZone("UTC"));

		/**
     * Element name of the packet extension.
     */
		public static readonly string ELEMENT_NAME = "x";

		/**
     * Namespace of the packet extension.
     */
		public static readonly string NAMESPACE = "jabber:x:data";

		static DataForm()
        {
			UTC_FORMAT.setTimeZone(TimeZone.getTimeZone("UTC"));
			// Register that DataForms uses the jabber:x:data namespace
			registeredExtensions.TryAdd(XName.Get(ELEMENT_NAME, NAMESPACE), typeof(DataForm));
		}

		/**
     * Returns the Date obtained by parsing the specified date representation. The date
     * representation is expected to be in the UTC GMT+0 format.
     *
     * @param date date representation in the UTC GMT+0 format.
     * @return the Date obtained by parsing the specified date representation.
     * @throws ParseException if an error occurs while parsing the date representation.
     */
		public static DateTime parseDate(string date)
        {
			lock (UTC_FORMAT) {
				return UTC_FORMAT.parse(date);
			}
		}

		public static bool parsebool(String boolString)
        {
			return "1".Equals(boolString) || "true".Equals(boolString);
		}

		/**
     * Returns the String representation of an Object to be used as a field value.
     *
     * @param object the object to encode.
     * @return the String representation of an Object to be used as a field value.
     */
		static string Encode(object @object) {
			if (@object is String) {
				return @object.ToString();
			}
			else if (@object is bool) {
				return true.Equals(@object) ? "1" : "0";
			}
			else if (@object is DateTime) {
				return FAST_UTC_FORMAT.format((Date) @object);
			}
			return @object.ToString();
		}

		public DataForm(Type type)
            : base(ELEMENT_NAME, NAMESPACE)
        {
			// Set the type of the data form
			element.Add(new XAttribute("type", type.ToString()));
		}

		public DataForm(XElement element)
            : base(element)
        { }

		/**
     * Returns the type of this data form.
     *
     * @return the data form type.
     * @see org.xmpp.forms.DataForm.Type
     */
		public DataForm.Type? GetType()
		{
		    string type = element.Attribute("type").Value;
			if (type != null) {
				return DataForm.Type.ValueOf(type);
			}
			else {
				return null;
			}
		}

		/**
     * Sets the description of the data. It is similar to the title on a web page or an X window.
     * You can put a <title/> on either a form to fill out, or a set of data results.
     *
     * @param title description of the data.
     */
		public void SetTitle(string title) {
			// Remove an existing title element.
		    var titleElement = element.Element("title");
			if (titleElement != null) {
				titleElement.Remove();
			}
			element.Add(new XElement("title", title));
		}

		/**
     * Returns the description of the data form. It is similar to the title on a web page or an X
     * window.  You can put a <title/> on either a form to fill out, or a set of data results.
     *
     * @return description of the data.
     */
		public string GetTitle()
		{
		    var titleElement = element.Element("title");
		    return titleElement != null ? titleElement.Value : "";
		}

		/**
     * Returns an unmodifiable list of instructions that explain how to fill out the form and
     * what the form is about. The dataform could include multiple instructions since each
     * instruction could not contain newlines characters.
     *
     * @return an unmodifiable list of instructions that explain how to fill out the form.
     */
		public List<string> GetInstructions() {
			var instructions = new List<String>();
		    foreach (var instructionElement in element.Elements("instructions"))
		    {
		        instructions.Add(instructionElement.Value);
		    }
		    return instructions;
		}

		/**
     * Adds a new instruction to the list of instructions that explain how to fill out the form
     * and what the form is about. The dataform could include multiple instructions since each
     * instruction could not contain newlines characters.
     * <p>
     * Nothing will be set, if the provided argument is <tt>null</tt> or an empty String.
     *
     * @param instruction the new instruction that explain how to fill out the form.
     */
		public void AddInstruction(string instruction) {
		    if (string.IsNullOrEmpty(instruction))
		    {
		        return;
		    }

			element.Add(new XElement("instructions", instruction));
		}

		/**
     * Clears all the stored instructions in this packet extension.
     */
		public void ClearInstructions() {
		    foreach (var instructionElement in element.Elements("instructions").ToList())
		    {
		        instructionElement.Remove();
		    }
		}

		/**
     * Adds a new field as part of the form.
     *
     * @return the newly created field.
     */
		public FormField AddField()
		{
		    var fieldElement = new XElement("field");
            element.Add(fieldElement);
			return new FormField(fieldElement);
		}

		/**
     * Adds a new field as part of the form. The provided arguments are optional 
     * (they are allowed to be <tt>null</tt>).
     *
     * @param variable the unique identifier of the field in the context of the 
     * 		form. Optional parameter.
     * @param type an indicative of the format for the data. Optional parameter. 
     * @param label the label of the question. Optional parameter.
     * @return the newly created field.
     */
		public FormField AddField(String variable, String label, FormField.Type? type) {
			FormField result = AddField();
		    if (!string.IsNullOrEmpty(variable))
		    {
		        result.SetVariable(variable);
		    }

			if (type != null) {
				result.SetType(type);
			}

			if (label != null && label.Trim().Length >= 0) {
				result.SetLabel(label);
			}

			return result;
		}

		/**
     * Returns the fields that are part of the form.
     *
     * @return fields that are part of the form.
     */
		public List<FormField> GetFields() {
			var fields = new List<FormField>();
		    foreach (var fieldElement in element.Elements("field"))
		    {
		        fields.Add(new FormField(fieldElement));
		    }
			return fields;
		}

		/**
     * Returns the field whose variable matches the specified variable.
     *
     * @param variable the variable name of the field to search.
     * @return the field whose variable matches the specified variable
     */
		public FormField GetField(string variable)
		{
		    return element.Elements("field")
		        .Select(x => new FormField(x))
		        .FirstOrDefault(x => x.GetVariable().Equals(variable));
		}

		/**
     * Removes the field whose variable matches the specified variable.
     *
     * @param variable the variable name of the field to remove.
     * @return true if the field was removed.
     */
		public bool RemoveField(string variable) {
		    foreach (var fieldElement in element.Elements("field").ToList())
		    {
		        var variableAttribute = fieldElement.Attribute("var");
		        if (variableAttribute != null && variable.Equals(variableAttribute.Value))
		        {
		            fieldElement.Remove();
		            return true;
		        }
		    }
			return false;
		}

		/**
     * Adds a field to the list of fields that will be returned from a search. Each field represents
     * a column in the report. The order of the columns in the report will honor the sequence in
     * which they were added.
     *
     * @param variable variable name of the new column. This value will be used in
     *       {@link #addItemFields} when adding reported items.
     * @param label label that corresponds to the new column. Optional parameter.
     * @param type indicates the type of field of the new column. Optional parameter.
     */
		public void AddReportedField(String variable, String label, FormField.Type type) {
			XElement reported = element.Element("reported");
			lock (element) {
				if (reported == null) {
					reported = element.Element("reported");
					if (reported == null) {
                        reported = new XElement("reported");
						element.Add(reported);
					}
				}
			}
		    var fieldElement = new XElement("field");
            reported.Add(fieldElement);

			FormField newField = new FormField(fieldElement);
			newField.SetVariable(variable);
			newField.SetType(type);
			newField.SetLabel(label);
		}

		/**
     * Adds a new row of items of reported data. For each entry in the <tt>fields</tt> parameter
     * a <tt>field</tt> element will be added to the <item> element. The variable of the new
     * <tt>field</tt> will be the key of the entry. The new <tt>field</tt> will have several values
     * if the entry's value is a {@link Collection}. Since the value is of type {@link Object} it
     * is possible to include any type of object as a value. The actual value to include in the
     * data form is the result of the {@link #encode(Object)} method.
     *
     * @param fields list of <variable,value> to be added as a new item.
     */
		public void AddItemFields(Dictionary<string,object> fields) {
            XElement item = new XElement("item");
			element.Add(item);
			// Add a field element to the item element for each row in fields
		    foreach (var entry in fields)
		    {
		        XElement field = new XElement("field",
                    new XAttribute("var", entry.Key));
		        item.Add(field);

		        var value = entry.Value;
                if (value == null)
                    continue;

		        if (value is CollectionBase)
		        {
		            foreach (var colValue in (value as CollectionBase))
		            {
		                if (colValue != null)
		                {
		                    var valueElement = new XElement("value", Encode(colValue));
                            field.Add(valueElement);
		                }
		            }
		        }
		        else
		        {
		            var valueElement = new XElement("value", Encode(value));
                    field.Add(valueElement);
		        }
		    }
		}

		public DataForm CreateCopy() {
			return new DataForm(this.getElement().CreateCopy());
		}

		/**
     * Type-safe enumeration to represent the type of the Data forms.
     */
		public enum Type {
			/**
         * The forms-processing entity is asking the forms-submitting entity to complete a form.
         */
			form,

			/**
         * The forms-submitting entity is submitting data to the forms-processing entity.
         */
			submit,

			/**
         * The forms-submitting entity has cancelled submission of data to the forms-processing
         * entity.
         */
			cancel,

			/**
         * The forms-processing entity is returning data (e.g., search results) to the
         * forms-submitting entity, or the data is a generic data set.
         */
			result
		}
	}
}

