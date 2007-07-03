using System; 
using System.Web;
using System.Web.Security; 
using System.Web.UI; 
using System.Web.UI.WebControls; 


namespace FlexWiki.Web.Admin
{
    public class Users : Page
    {
        protected GridView UserList; 

        public void Page_Load(object sender, EventArgs a)
        {
            UserList.DataSource = Membership.GetAllUsers();
            DataBind(); 
        }
    }
}