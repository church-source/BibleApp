using System.Text;
/*
Software Copyright Notice

Copyright © 2004-2010 MXit Lifestyle Development Company (Pty) Ltd.
All rights are reserved

Copyright exists in this computer program and it is protected by
copyright law and by international treaties. The unauthorised use,
reproduction or distribution of this computer program constitute
acts of copyright infringement and may result in civil and criminal
penalties. Any infringement will be prosecuted to the maximum extent
possible.

MXit Lifestyle Development Company (Pty) Ltd chooses the following
address for delivery of all legal proceedings and notices:
  Riesling House,
  Brandwacht Office Park,
  Trumali Road,
  Stellenbosch,
  7600,
  South Africa.
*/
using MXit.Common;

namespace MXit.ExternalApp
{
    /// <summary>
    /// A base class for ExternalApp user session classes to extend from.
    /// </summary>
    public class ExternalAppUserSession : WithExpandedToString
    {
        #region Variables & Properties

        /// <summary>
        /// The contact name (i.e. the context) in which the user is accessing your ExternalApp.
        /// </summary>
        public string ContactName { get; internal set; }

        #endregion

        public virtual void logSessionEnd()
        {
            //do nothing. 
        }

        #region ToString

        /// <summary>
        /// Adds key-value items to the output produced by a call to <see cref="M:MXit.Common.WithExpandedToString.ToString"/>.
        /// </summary>
        /// <param name="sb">The string builder to add the key-value items to.</param>
        /// <param name="itemFormat">The key-value item format.</param>
        /// <param name="indent">Number of indentations.</param>
        /// <param name="spacer">String to use as indentation spacer.</param>
        /// <param name="equals">String to use a equals sign.</param>
        /// <param name="preText">The pre-text before each entry per line.</param>
        /// <example>
        /// 	<code>
        /// // This function will typically contain a series of name/value entries, e.g.
        /// sb.AppendFormat(itemFormat, "Name", Name);
        /// sb.AppendFormat(itemFormat, "Port", Port);
        /// sb.AppendFormat(itemFormat, "Host", System.Net.Dns.GetHostName());
        /// </code>
        /// </example>
        public override void ToStringAddKeyValueItems(StringBuilder sb, string itemFormat, int indent, string spacer, string equals, string preText)
        {
            sb.AppendFormat(itemFormat, "ContactName", ContactName);
        }

        #endregion
    }
}
