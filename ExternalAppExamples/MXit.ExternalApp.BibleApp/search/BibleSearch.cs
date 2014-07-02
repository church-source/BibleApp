using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sphinx.Client.Commands.Search;
using Sphinx.Client.Connections;

namespace MxitTestApp
{
    class BibleSearch
    {
        private static BibleSearch instance;

        static BibleSearch()
        {
            getInstance();
        }

        public BibleSearch()
        {
        }

        public static BibleSearch getInstance()
        {
            if (instance != null)
                return instance;
            else
            {
                //TODO synchronize this method
                instance = new BibleSearch();
                return instance;
            }
        }

        public IList<SearchQueryResult> searchBible(
            String searchString, 
            int translation, 
            int bookID,
            int testament)
        {
            using (ConnectionBase connection = new PersistentTcpConnection("127.0.0.1", 9312))
            {
                // Create new search query object and pass query text as argument
                SearchQuery searchQuery = new SearchQuery(searchString);
                // Set match mode to SPH_MATCH_EXTENDED2
                searchQuery.MatchMode = MatchMode.All;
                // Add Sphinx index name to list
                searchQuery.Indexes.Add("test1");
                // Setup attribute 
                searchQuery.AttributeFilters.Add("translation", translation, false);
                if (bookID != -1)
                {
                    searchQuery.AttributeFilters.Add("book", bookID, false);
                }

                if (testament != -1)
                {
                    searchQuery.AttributeFilters.Add("testament", testament, false);
                }
                // Set amount of matches will be returned to client 
                searchQuery.Limit = 50;

                // Create search command object
                SearchCommand searchCommand = new SearchCommand(connection);
                // Add newly created search query object to query list
                searchCommand.QueryList.Add(searchQuery);
                // Execute command on server and obtain results
                searchCommand.Execute();
                return searchCommand.Result.QueryResults;
            }
        }
    }
}
