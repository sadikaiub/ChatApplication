using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using SD = System.Drawing;
using System.Web.UI.HtmlControls;
using System.Configuration;

namespace SignalRChat
{
    public partial class Chat : System.Web.UI.Page
    {
        

        public string UserName = "admin";
        public string UserImage = "/images/DP/dummy.png";
        protected string UploadFolderPath = "~/Uploads/";
        ConnClass ConnC = new ConnClass();
        protected void Page_Load(object sender, EventArgs e)
        {

            string conStr = ConfigurationManager.ConnectionStrings["conStr"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            SqlDataAdapter da = new SqlDataAdapter("select UserName,Message,GETDATE() as Date  from tbl_History", con);

            DataSet ds1= new DataSet();
            da.Fill(ds1);
            GridView.DataSource = ds1;
            GridView.DataBind();

            if (Session["UserName"] != null)
            {
                UserName = Session["UserName"].ToString();
                GetUserImage(UserName);

            }
            else
                Response.Redirect("Login.aspx");


            this.Header.DataBind();


        }

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        public void GetUserImage(string Username)
        {
            if (Username != null)
            {
                string query = "select Photo from tbl_Users where UserName='" + Username + "'";

                string ImageName = ConnC.GetColumnVal(query, "Photo");
                if (ImageName != "")
                    UserImage = "images/DP/" + ImageName;
            }


        }
        

        protected void btnSendMsg_ServerClick(object sender, EventArgs e)
        {

            string Query = "Insert into tbl_History (UserName,Message) values ('" +UserName+ "','" + txtMessage.Value + "' )";
            ConnC.IsExist(Query);


            //if (ConnC.IsExist(Query))
            //{
            //    string UserName = ConnC.GetColumnVal(Query, "UserName");
            //    Session["UserName"] = UserName;
            //    Session["Message"] = Request.Form["Message"];
            //    Response.Redirect("Chat.aspx");
            //}

        }

        protected void btnSendMsgP_ServerClick(object sender, EventArgs e)
        {

            //string Query = "Insert into tbl_Private_History (From_User_Id,To_User_Id,Message_Creation_Date,Message) values ('"++"',1,'09-03-2019','"+txtMessage.+"')";
            //
            //ConnC.IsExist(Query);

        }

        protected void btnChangePicModel_Click(object sender, EventArgs e)
        {

            string serverPath = HttpContext.Current.Server.MapPath("~/");
            //path = serverPath + path;
            if (FileUpload1.HasFile)
            {
                string FileWithPat = serverPath + @"images/DP/" + UserName + FileUpload1.FileName;

                FileUpload1.SaveAs(FileWithPat);
                SD.Image img = SD.Image.FromFile(FileWithPat);
                SD.Image img1 = RezizeImage(img, 151, 150);
                img1.Save(FileWithPat);
                if (File.Exists(FileWithPat))
                {
                    FileInfo fi = new FileInfo(FileWithPat);
                    string ImageName = fi.Name;
                    string query = "update tbl_Users set Photo='" + ImageName + "' where UserName='" + UserName + "'";
                    if (ConnC.ExecuteQuery(query))
                        UserImage = "images/DP/" + ImageName;
                }
            }
        }


        #region Resize Image With Best Qaulity

        private SD.Image RezizeImage(SD.Image img, int maxWidth, int maxHeight)
        {
            if (img.Height < maxHeight && img.Width < maxWidth) return img;
            using (img)
            {
                Double xRatio = (double)img.Width / maxWidth;
                Double yRatio = (double)img.Height / maxHeight;
                Double ratio = Math.Max(xRatio, yRatio);
                int nnx = (int)Math.Floor(img.Width / ratio);
                int nny = (int)Math.Floor(img.Height / ratio);
                Bitmap cpy = new Bitmap(nnx, nny, SD.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(cpy))
                {
                    gr.Clear(Color.Transparent);

                    // This is said to give best quality when resizing images
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    gr.DrawImage(img,
                        new Rectangle(0, 0, nnx, nny),
                        new Rectangle(0, 0, img.Width, img.Height),
                        GraphicsUnit.Pixel);
                }
                return cpy;
            }

        }

        private MemoryStream BytearrayToStream(byte[] arr)
        {
            return new MemoryStream(arr, 0, arr.Length);
        }


        #endregion

        protected void FileUploadComplete(object sender, EventArgs e)
        {
            string filename = System.IO.Path.GetFileName(AsyncFileUpload1.FileName);
            AsyncFileUpload1.SaveAs(Server.MapPath(this.UploadFolderPath) + filename);
        }

        

    }
}