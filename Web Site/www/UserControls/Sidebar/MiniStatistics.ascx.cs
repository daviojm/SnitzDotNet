/*
####################################################################################################################
##
## SnitzUI.UserControls.Sidebar - MiniStatistics.ascx
##   
## Author:		Huw Reddick
## Copyright:	Huw Reddick
## based on code from Snitz Forums 2000 (c) Huw Reddick, Michael Anderson, Pierre Gorissen and Richard Kinser
## Created:		29/07/2013
## 
## The use and distribution terms for this software are covered by the 
## Eclipse License 1.0 (http://opensource.org/licenses/eclipse-1.0)
## which can be found in the file Eclipse.txt at the root of this distribution.
## By using this software in any fashion, you are agreeing to be bound by 
## the terms of this license.
##
## You must not remove this notice, or any other, from this software.  
##
#################################################################################################################### 
*/

using System;
using Resources;
using Snitz.BLL;
using Snitz.Entities;
using SnitzCommon;
using Snitz.Providers;



public partial class MiniStatistics : System.Web.UI.UserControl
{
    
    private const string ProfileUrl = "<a href=\"/Account/profile.aspx?user={0}\" title=\"{1}\" rel=\"NoFollow\">{0}</a>";
    
    private PageBase _page;
    private StatsInfo stats;

    private string NewMember
    {
        set
        {
            lblNewestMember.Text = value;
        }
    }

    private string MemberStats
    {
        set
        {
            lblMemberStats.Text = value;
        }
    }

    private string TopicStats
    {
        set
        {
            lblTopicStats.Text = value;
        }
    }

    private string ActiveUsers
    {
        set { lblActiveUsers.Text = value;}
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["CurrentProfile"] != null)
            Session.Remove("CurrentProfile");
        _page = (PageBase) Page;
        stats = Statistics.GetStatistics();
        string newmemberlink = String.Format(ProfileUrl, stats.NewestMember, String.Format(webResources.lblViewProfile, stats.NewestMember));
        NewMember = string.Format(webResources.lblMiniStatsNewMember, newmemberlink);

        int memberCount = stats.MemberCount;
        int totalPostCount = stats.TotalPostCount;

        MemberStats = string.Format(webResources.lblMiniStatsMembers, Common.TranslateNumerals(memberCount), Common.TranslateNumerals(totalPostCount), GetLastPost(), GetLastPostAuthor());
        TopicStats = string.Format(webResources.lblMiniStatsTopics, Common.TranslateNumerals(stats.ActiveTopicCount));
        int activemembers = new SnitzMembershipProvider().GetNumberOfUsersOnline();
        int totalsessions = Convert.ToInt32(Application["SessionCount"]);
        int anonusers = totalsessions - activemembers;
        lblActiveSessions.Text = extras.GuestLabel + Common.TranslateNumerals(anonusers);
        ActiveUsers = string.Format(webResources.lblStatsMembersOnline,
                                    String.Join(",", new SnitzMembershipProvider().GetOnlineUsers()));
    }

    private string GetLastPost()
    {
        if (stats.LastPost == null)
            return "";
        const string url = "<a href=\"/Content/Forums/topic.aspx?TOPIC={0}&whichpage=-1#{1}\">{2}</a>";
        TopicInfo lastpost = stats.LastPost;

        if (lastpost.LastPostDate != null)
        {
            string lastpostDate = Common.TimeAgoTag(lastpost.LastPostDate.Value, ((PageBase)Page).IsAuthenticated, _page.Member != null ? _page.Member.TimeOffset : 0);
            return String.Format(url, lastpost.Id, lastpost.LastReplyId, lastpostDate);
        }
        return String.Empty;
    }
    
    private string GetLastPostAuthor() 
    {
        var author = stats.LastPostAuthor;
        return author == null ? String.Empty : String.Format(ProfileUrl, author.Username, String.Format(webResources.lblViewProfile,author.Username));
    }

}
