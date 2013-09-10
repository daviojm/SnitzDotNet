﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Snitz.Entities;
using Snitz.IDAL;
using Snitz.SQLServerDAL.Helpers;
using SnitzConfig;

namespace Snitz.SQLServerDAL
{
    public class Topic : ITopic
    {
        public const string TopicCols = 
            " T.TOPIC_ID,T.CAT_ID,T.FORUM_ID,T.T_STATUS,T.T_SUBJECT,T.T_AUTHOR,T.T_REPLIES,T.T_VIEW_COUNT,T.T_LAST_POST" +
            ",T.T_DATE,T.T_IP,T.T_LAST_POST_AUTHOR,T.T_STICKY,T.T_LAST_EDIT,T.T_LAST_EDITBY,T.T_SIG,T.T_LAST_POST_REPLY_ID" +
            ",T.T_UREPLIES,T.T_MESSAGE,A.M_NAME AS Author, LPA.M_NAME AS LastPostAuthor, EM.M_NAME AS Editor,A.M_VIEW_SIG, A.M_SIG ";
        public const string TopicFrom = 
            "FROM FORUM_TOPICS T " + 
            "LEFT OUTER JOIN FORUM_MEMBERS LPA ON T.T_LAST_POST_AUTHOR = LPA.MEMBER_ID " +
            "LEFT OUTER JOIN FORUM_MEMBERS AS A ON T.T_AUTHOR = A.MEMBER_ID " +
            "LEFT OUTER JOIN FORUM_MEMBERS AS EM ON T.T_LAST_EDITBY = EM.MEMBER_ID ";

        #region ITopic Members

        public IEnumerable<ReplyInfo> GetReplies(int topicid, int startrec, int maxrecs)
        {
            return new Reply().GetByParent(topicid, startrec, maxrecs);
        }

        public TopicInfo GetNextPrevTopic(int topicid, string which)
        {
            string strSql = "SELECT TOP 1 " + TopicCols + TopicFrom;
            TopicInfo topic = GetById(topicid);
            

            if (which == "next")
            {
                strSql = strSql + " WHERE T.FORUM_ID=@ForumId AND T.T_LAST_POST > @TopicDate ORDER BY T.T_LAST_POST";
            }
            else
            {
                strSql = strSql + " WHERE T.FORUM_ID=@ForumId AND T.T_LAST_POST < @TopicDate ORDER BY T.T_LAST_POST DESC";
                
            }
            TopicInfo newtopic = null;
            List<SqlParameter> parms = new List<SqlParameter>
                                       {
                                           new SqlParameter("@ForumId", SqlDbType.Int){Value=topic.Id}
                                       };
            parms.Add(topic.LastPostDate.HasValue
                ? new SqlParameter("@TopicDate", SqlDbType.Int){Value =topic.LastPostDate.Value.ToString("yyyyMMddHHmmss")}
                : new SqlParameter("@TopicDate", SqlDbType.Int) {Value = topic.Date.ToString("yyyyMMddHHmmss")});
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, parms.ToArray()))
            {
                while (rdr.Read())
                {
                    newtopic = BoHelper.CopyTopicToBO(rdr);
                }
            }
            return newtopic;
        }

        public void UpdateLastTopicPost(ReplyInfo reply)
        {
            const string updateTopicSql = "UPDATE FORUM_TOPICS SET T_REPLIES=T_REPLIES+1,T_LAST_POST_REPLY_ID=@ReplyId,T_LAST_POST=@ReplyDate,T_LAST_POST_AUTHOR=@ReplyAuthor WHERE TOPIC_ID=@TopicId; ";
            List<SqlParameter> parms = new List<SqlParameter>
                {
                    new SqlParameter("@TopicId", SqlDbType.Int) {Value = reply.TopicId},
                    new SqlParameter("@ReplyId", SqlDbType.Int) {Value = reply.Id},
                    new SqlParameter("@ReplyDate", SqlDbType.VarChar) {Value = reply.Date.ToString("yyyyMMddHHmmss")},
                    new SqlParameter("@ReplyAuthor", SqlDbType.Int) {Value = reply.AuthorId}
                };

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, updateTopicSql, parms.ToArray());
        }

        public void SetTopicStatus(int topicid, int status)
        {
            const string updateSql = "UPDATE FORUM_TOPICS SET T_STATUS=@Status WHERE TOPIC_ID=@TopicId";
            List<SqlParameter> parms = new List<SqlParameter>
                {
                    new SqlParameter("@TopicId", SqlDbType.Int) {Value = topicid},
                    new SqlParameter("@Status", SqlDbType.Int) {Value = status}
                };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, updateSql, parms.ToArray());
        }

        public void MakeSticky(int topicid, bool sticky)
        {
            const string updateSql = "UPDATE FORUM_TOPICS SET T_STICKY=@Sticky WHERE TOPIC_ID=@TopicId";
            List<SqlParameter> parms = new List<SqlParameter>
                {
                    new SqlParameter("@TopicId", SqlDbType.Int) {Value = topicid},
                    new SqlParameter("@Sticky", SqlDbType.Bit) {Value = sticky}
                };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, updateSql, parms.ToArray());
        }

        public void ChangeTopicForum(int topicid, int forumid)
        {
            ForumInfo newforum = new Forum().GetById(forumid);

            string updateSql = "UPDATE FORUM_TOPICS SET FORUM_ID=@ForumId, CAT_ID=@CatId WHERE TOPIC_ID=@TopicId; ";
            if (newforum.Status == (int)Enumerators.PostStatus.Closed) //forum locked so lock all posts
            {
                updateSql = updateSql + "UPDATE FORUM_TOPICS SET T_STATUS=0 WHERE TOPIC_ID=@TopicId; ";
                updateSql = updateSql + "UPDATE FORUM_REPLY SET R_STATUS=0 WHERE TOPIC_ID=@TopicId; ";                
            }
            else if (newforum.ModerationLevel == (int) Enumerators.Moderation.UnModerated)
            {
                //change status of posts if coming from moderated forum
                updateSql = updateSql + "UPDATE FORUM_TOPICS SET T_STATUS=1 WHERE TOPIC_ID=@TopicId AND T_STATUS > 1; ";
                updateSql = updateSql + "UPDATE FORUM_REPLY SET R_STATUS=1 WHERE TOPIC_ID=@TopicId AND R_STATUS > 1; ";

            }
            updateSql = updateSql + "UPDATE FORUM_REPLY SET FORUM_ID=@ForumId, CAT_ID=@CatId WHERE TOPIC_ID=@TopicId; ";
            List<SqlParameter> parms = new List<SqlParameter>
                {
                    new SqlParameter("@TopicId", SqlDbType.Int) {Value = topicid},
                    new SqlParameter("@ForumId", SqlDbType.Int) {Value = forumid},
                    new SqlParameter("@CatId", SqlDbType.Int) {Value = newforum.CatId}
                };
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, updateSql, parms.ToArray());
        }

        public void UpdateViewCount(int topicid)
        {
            SqlParameter parm = new SqlParameter("@TopicId", SqlDbType.Int) {Value = topicid};
            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, "UPDATE FORUM_TOPICS SET T_VIEW_COUNT = T_VIEW_COUNT + 1 WHERE TOPIC_ID=@TopicId", parm);
        }

        public IEnumerable<TopicInfo> GetTopicsForSiteMap(int maxRecords)
        {
            List<TopicInfo> topics = new List<TopicInfo>();

            string strSql = "SELECT TOP " + maxRecords + " " + TopicCols + TopicFrom + " WHERE T.T_STATUS<2 ORDER BY T.T_LAST_POST DESC";
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, null)) 
            {
                while (rdr.Read())
                {
                    topics.Add(BoHelper.CopyTopicToBO(rdr));
                }
            }

            return topics;
        }

        public IEnumerable<TopicInfo> GetLatestTopics(int topicCount, string lastvisit)
        {
            List<TopicInfo> topics = new List<TopicInfo>();
            List<SqlParameter> parms = new List<SqlParameter>();
            string strSql = "";
            const string @join = " LEFT OUTER JOIN FORUM_CATEGORY C ON T.CAT_ID = C.CAT_ID LEFT OUTER JOIN FORUM_FORUM F ON T.FORUM_ID = F.FORUM_ID ";
            string where = "";
            const string @orderby = " ORDER BY T.T_LAST_POST DESC";

            if (topicCount > 0)
            {
                parms.Add(new SqlParameter("@RecCount", SqlDbType.Int) { Value = topicCount });
                strSql = "SELECT TOP(@RecCount) ";
            }
            if (!String.IsNullOrEmpty(lastvisit))
            {
                parms.Add(new SqlParameter("@LastVisit", SqlDbType.VarChar) {Value = lastvisit});
                strSql = "SELECT ";
                where = "WHERE T_STATUS=1 AND T.T_LAST_POST > @LastVisit";
            }
            strSql = strSql + TopicCols + TopicFrom + join + where + orderby; 
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, parms.ToArray()))
            {
                while (rdr.Read())
                {
                    topics.Add(BoHelper.CopyTopicToBO(rdr));
                }
            }

            return topics;
        }

        public int? GetPollId(int topicid)
        {
            const string strSql = "SELECT PollId FROM FORUM_POLLS WHERE TopicId=@TopicId";

            var res = SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, strSql,new SqlParameter("@TopicId", SqlDbType.Int) {Value = topicid});
            int? pollid;
            if (res != null)
                pollid = Convert.ToInt32(res);
            else
            {
                pollid = null;
            }
            return pollid;
        }

        public IEnumerable<SearchResult> FindTopics(SearchParamInfo sparams, int currentPage, string orderby, ref int rowcount)
        {
            string whereClause = "";
            string orderfield = "";
            string whereOperator = "WHERE";
            List<SqlParameter> param = new List<SqlParameter>();

            switch (orderby)
            {
                case "Subject" :
                    orderfield = "TT.T_SUBJECT";
                    break;
                case "Replies" :
                    orderfield = "TT.T_REPLIES DESC";
                    break;
                case "Views" :
                    orderfield = "TT.T_VIEW_COUNT DESC";
                    break;
                case "Date" :
                    orderfield = "TT.T_DATE DESC";
                    break;
                case "Author" :
                    orderfield = "TM.M_NAME";
                    break;
                case "ForumOrder" :

                    break;
                default :
                    orderfield = "TT.T_LAST_POST DESC";
                    break;
            }
            StringBuilder selectover = new StringBuilder();
            selectover.AppendLine("WITH TopicEntities AS (");
            selectover.AppendLine("SELECT TT.TOPIC_ID, DENSE_RANK() OVER (ORDER BY " + orderfield + ") AS RowNo");
            selectover.AppendLine("FROM FORUM_TOPICS TT ");
            selectover.AppendLine("LEFT JOIN FORUM_REPLY TR ON TT.TOPIC_ID = TR.TOPIC_ID ");
            selectover.AppendLine("LEFT JOIN FORUM_CATEGORY TC ON TT.CAT_ID = TC.CAT_ID");
            selectover.AppendLine("LEFT JOIN FORUM_FORUM TF ON TT.FORUM_ID = TF.FORUM_ID ");
            selectover.AppendLine("LEFT JOIN FORUM_MEMBERS TM ON TT.T_AUTHOR = TM.MEMBER_ID ");
            selectover.AppendLine("LEFT JOIN FORUM_MEMBERS TRA ON TR.R_AUTHOR = TRA.MEMBER_ID ");
            selectover.AppendLine("LEFT JOIN FORUM_MEMBERS MEMBERS_1 ON TT.T_LAST_POST_AUTHOR = MEMBERS_1.MEMBER_ID ");
            

            const string strSql = "SELECT DISTINCT TE.RowNo, C.CAT_STATUS, C.CAT_SUBSCRIPTION, C.CAT_NAME " +
                                  ", F.F_SUBJECT, F.F_SUBSCRIPTION, F.F_STATUS, F.F_PRIVATEFORUMS, F.F_PASSWORD_NEW " +
                                  ", T.CAT_ID, T.FORUM_ID, T.TOPIC_ID, T.T_AUTHOR, T.T_SUBJECT, T.T_STATUS, T.T_LAST_POST " +
                                  ", T.T_LAST_POST_AUTHOR, T.T_LAST_POST_REPLY_ID, T.T_REPLIES, T.T_UREPLIES, T.T_VIEW_COUNT " +
                                  ", TA.MEMBER_ID, TA.M_NAME, LRA.M_NAME AS LAST_POST_AUTHOR_NAME,(select count(*) from TopicEntities) as TotalRows " +
                                  "FROM TopicEntities TE  " +
                                  "INNER JOIN FORUM_TOPICS T on TE.TOPIC_ID = T.TOPIC_ID  " +
                                  "LEFT JOIN FORUM_REPLY R ON T.TOPIC_ID = R.TOPIC_ID " +
                                  "LEFT JOIN FORUM_CATEGORY C ON T.CAT_ID = C.CAT_ID " +
                                  "LEFT JOIN FORUM_FORUM F ON T.FORUM_ID = F.FORUM_ID " +
                                  "LEFT JOIN FORUM_MEMBERS TA ON T.T_AUTHOR = TA.MEMBER_ID " +
                                  "LEFT JOIN FORUM_MEMBERS RA ON R.R_AUTHOR = RA.MEMBER_ID " +
                                  "LEFT JOIN FORUM_MEMBERS LRA ON T.T_LAST_POST_AUTHOR = LRA.MEMBER_ID " +
                                  "WHERE TE.RowNo Between @Start AND @MaxRows ORDER BY TE.RowNo ";

            if (sparams.ForumId > 0)
            {
                param.Add(new SqlParameter("@ForumId", SqlDbType.Int) { Value = sparams.ForumId });
                whereClause = whereClause + whereOperator + " TT.FORUM_ID=@ForumId ";
                whereOperator = "AND";
            }

            if (sparams.SinceDate != DateTime.MinValue)
            {
                param.Add(new SqlParameter("@LastVisit", SqlDbType.VarChar) { Value = sparams.SinceDate });
                whereClause = whereClause + whereOperator + " COALESCE(TT.T_LAST_POST,T.T_DATE) > @LastVisit ";
                whereOperator = "AND";
            }
            if (sparams.BeforeDate != DateTime.MinValue)
            {
                param.Add(new SqlParameter("@BeforeDate", SqlDbType.VarChar) { Value = sparams.SinceDate });
                whereClause = whereClause + whereOperator + " COALESCE(TT.T_LAST_POST,T.T_DATE) < @BeforeDate ";
                whereOperator = "AND";
            }
            if (!String.IsNullOrEmpty(sparams.Author))
            {
                if (sparams.AuthorPostType == "topic")
                {
                    param.Add(new SqlParameter("@Author", SqlDbType.VarChar) {Value = sparams.Author});
                    whereClause = whereClause + whereOperator + " TM.M_NAME=@Author ";
                }
                else if (sparams.AuthorPostType == "any")
                {
                    param.Add(new SqlParameter("@Author", SqlDbType.VarChar) { Value = sparams.Author });
                    whereClause = whereClause + whereOperator + " (TM.M_NAME=@Author OR TRA.M_NAME=@Author) ";                   
                }
                whereOperator = "AND";
            }

            if (!String.IsNullOrEmpty(sparams.SearchFor))
            {
                whereClause = whereClause + whereOperator + " (";

                string[] searchTerms;
                switch (sparams.Match)
                {
                    case "any":
                        whereOperator = "";
                        searchTerms = sparams.SearchFor.Split(new[] { ' ' });
                        if (sparams.MessageAndSubject)
                        {
                            for (int i = 0; i < searchTerms.Length; i++)
                            {
                                param.Add(new SqlParameter("@SearchFor" + i, SqlDbType.NVarChar) { Value = "%" + searchTerms[i] + "%" });
                                whereClause = whereClause + whereOperator + "(TT.T_SUBJECT LIKE @SearchFor" + i + " OR TT.T_MESSAGE LIKE @SearchFor" + i + ")";
                                whereOperator = "OR";
                            }
                        }
                        else
                        {
                            for (int i = 0; i < searchTerms.Length; i++)
                            {
                                param.Add(new SqlParameter("@SearchFor" + i, SqlDbType.NVarChar) { Value = "%" + searchTerms[i] + "%" });
                                whereClause = whereClause + whereOperator + "TT.T_SUBJECT LIKE @SearchFor" + i + " ";
                                whereOperator = "OR";
                            }
                        }

                        break;
                    case "all":
                        whereOperator = " (";
                        searchTerms = sparams.SearchFor.Split(new[] { ' ' });
                        if (sparams.MessageAndSubject)
                        {
                            for (int i = 0; i < searchTerms.Length; i++)
                            {
                                param.Add(new SqlParameter("@SearchFor" + i, SqlDbType.NVarChar) { Value = "%" + searchTerms[i] + "%" });
                                whereClause = whereClause + whereOperator + " TT.T_SUBJECT LIKE @SearchFor" + i + " ";
                                whereOperator = "AND";
                            }
                            whereOperator = ") OR (";
                            for (int i = 0; i < searchTerms.Length; i++)
                            {
                                param.Add(new SqlParameter("@SearchFor" + (i + searchTerms.Length), SqlDbType.NVarChar) { Value = "%" + searchTerms[i] + "%" });
                                whereClause = whereClause + whereOperator + " TT.T_MESSAGE LIKE @SearchFor" + (i + searchTerms.Length) + " ";
                                whereOperator = "AND";
                            }
                            whereClause = whereClause + ") ";
                        }
                        else
                        {
                            whereOperator = " (";
                            for (int i = 0; i < searchTerms.Length; i++)
                            {
                                param.Add(new SqlParameter("@SearchFor" + i, SqlDbType.NVarChar) { Value = "%" + searchTerms[i] + "%" });
                                whereClause = whereClause + whereOperator + " TT.T_SUBJECT LIKE @SearchFor" + i + " ";
                                whereOperator = "AND";
                            }
                            whereClause = whereClause + ") ";
                        }
                        break;
                    default:
                        if (sparams.MessageAndSubject)
                        {
                            param.Add(new SqlParameter("@SearchFor", SqlDbType.NVarChar) { Value = "%" + sparams.SearchFor + "%" });
                            whereClause = whereClause + " TT.T_SUBJECT LIKE @SearchFor OR TT.T_MESSAGE LIKE @SearchFor";
                        }
                        else
                        {
                            param.Add(new SqlParameter("@SearchFor", SqlDbType.NVarChar) { Value = "%" + sparams.SearchFor + "%" });
                            whereClause = whereClause + " TT.T_SUBJECT LIKE @SearchFor";
                        }
                        break;
                }
                whereClause = whereClause + ") ";
            }

            selectover.AppendLine(whereClause + ")");

            List<SearchResult> topics = new List<SearchResult>();

            param.Add(new SqlParameter("@Start", SqlDbType.Int) { Value = currentPage*sparams.PageSize });
            param.Add(new SqlParameter("@MaxRows", SqlDbType.VarChar) { Value = currentPage * sparams.PageSize + sparams.PageSize });

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, selectover + strSql, param.ToArray()))
            {
                while (rdr.Read())
                {
                    topics.Add(BoHelper.CopySearchResultToBO(rdr,ref rowcount));
                }
            }
            return topics;

        }

        public IEnumerable<int> GetReplyIdList(int topicid)
        {
            const string strSql = "SELECT REPLY_ID FROM FORUM_REPLY WHERE TOPIC_ID=@TopicId ORDER BY R_DATE DESC";
            List<int> replies = new List<int>();
            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, strSql, new SqlParameter("@TopicId",SqlDbType.Int){Value = topicid}))
            {
                while (rdr.Read())
                {
                    replies.Add(rdr.GetInt32(0));
                }
            }
            return replies;
        }

        public IEnumerable<TopicInfo> GetTopics(string lastHereDate, int start, int maxrecs, int? forumid, bool isAdminOrModerator, int? topicstatus)
        {
            List<SqlParameter> param = new List<SqlParameter>();

            string selectover = "WITH TopicEntities AS (SELECT ROW_NUMBER() OVER (ORDER BY FORUM_ID,T_LAST_POST DESC,T_SUBJECT) AS Row, TOPIC_ID FROM FORUM_TOPICS WHERE T_STICKY=0 " ;
            const string strFrom = "FROM TopicEntities TE INNER JOIN FORUM_TOPICS T on TE.TOPIC_ID = T.TOPIC_ID " +
                                 "LEFT OUTER JOIN FORUM_MEMBERS LPA ON T.T_LAST_POST_AUTHOR = LPA.MEMBER_ID " +
                                 "LEFT OUTER JOIN FORUM_MEMBERS AS A ON T.T_AUTHOR = A.MEMBER_ID " +
                                 "LEFT OUTER JOIN FORUM_MEMBERS AS EM ON T.T_LAST_EDITBY = EM.MEMBER_ID " +
                                 "WHERE TE.Row Between @Start AND @MaxRows ORDER BY TE.Row ASC";

            //param.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = topicstatus });
            if (topicstatus.HasValue)
            {
                //+ (isAdminOrModerator ? "" : "AND T_STATUS=1")
                switch (topicstatus.Value)
                {
                    case 0:
                        param.Add(new SqlParameter("@Status", SqlDbType.Int) {Value = topicstatus.Value});
                        selectover = selectover + " AND T_STATUS=@Status";
                        break;
                    case 1:
                        param.Add(new SqlParameter("@Status", SqlDbType.Int) {Value = topicstatus.Value});
                        selectover = selectover + " AND T_STATUS=@Status";
                        break;
                    case 2:
                        param.Add(new SqlParameter("@Status", SqlDbType.Int) {Value = topicstatus.Value});
                        selectover = selectover + " AND T_STATUS=@Status";
                        break;
                }
            }
            else
            {
                if (!isAdminOrModerator)
                {
                    selectover = selectover + " AND T_STATUS < 2";                    
                }
            }
            if (forumid.HasValue)
            {
                param.Add(new SqlParameter("@ForumId", SqlDbType.Int) {Value = forumid});
                selectover = selectover + " AND FORUM_ID=@ForumId";
            }

            if (!String.IsNullOrEmpty(lastHereDate))
            {
                param.Add(new SqlParameter("@LastVisit",SqlDbType.VarChar) {Value = lastHereDate});
                selectover = selectover + " AND COALESCE(T_LAST_POST,T_DATE) > @LastVisit";
            }
            selectover = selectover + ")";

            List<TopicInfo> topics = new List<TopicInfo>();

            param.Add(new SqlParameter("@Start", SqlDbType.Int) {Value = start + 1});
            param.Add(new SqlParameter("@MaxRows", SqlDbType.VarChar) {Value = start + maxrecs});

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, selectover + "SELECT" + TopicCols + strFrom, param.ToArray()))
            {
                while (rdr.Read())
                {
                    topics.Add(BoHelper.CopyTopicToBO(rdr));
                }
            }
            return topics;
        }

        public int GetTopicCount(string lastHereDate, int start, int maxrecs, int? forumid, bool isAdminOrModerator, int? topicstatus)
        {
            string strSql =
                "SELECT COUNT(TOPIC_ID) FROM FORUM_TOPICS " +
                " WHERE T_STICKY=0 ";

            List<SqlParameter> param = new List<SqlParameter>();
            if (topicstatus.HasValue)
            {
                //+ (isAdminOrModerator ? "" : "AND T_STATUS=1")
                switch (topicstatus.Value)
                {
                    case 0:
                        param.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = topicstatus.Value });
                        strSql = strSql + " AND T_STATUS=@Status";
                        break;
                    case 1:
                        param.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = topicstatus.Value });
                        strSql = strSql + " AND T_STATUS=@Status";
                        break;
                    case 2:
                        param.Add(new SqlParameter("@Status", SqlDbType.Int) { Value = topicstatus.Value });
                        strSql = strSql + " AND T_STATUS=@Status";
                        break;
                }
            }
            else
            {
                if (!isAdminOrModerator)
                {
                    strSql = strSql + " AND T_STATUS < 2";
                }
            }
            if (forumid.HasValue)
            {
                param.Add(new SqlParameter("@ForumId", SqlDbType.Int) { Value = forumid });
                strSql = strSql + " AND FORUM_ID=@ForumId";
            }
            if (!String.IsNullOrEmpty(lastHereDate))
            {
                strSql = strSql + " AND T_LAST_POST > @LastVisit";
                param.Add(new SqlParameter("@LastVisit", SqlDbType.VarChar) {Value = lastHereDate});
            }

            var count = SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, strSql, param.ToArray());
            return Convert.ToInt32(count);
        }

        #endregion

        #region IBaseObject<TopicInfo> Members

        public TopicInfo GetById(int id)
        {
            TopicInfo topic = null;

            SqlParameter parm = new SqlParameter("@TopicId", SqlDbType.Int) {Value = id};

            using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnString, CommandType.Text, "SELECT" + TopicCols + TopicFrom + "WHERE T.TOPIC_ID = @TopicId", parm))
            {
                while (rdr.Read())
                {
                    topic = BoHelper.CopyTopicToBO(rdr);
                }
            }
            return topic;
        }

        public IEnumerable<TopicInfo> GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public int Add(TopicInfo topic)
        {
            StringBuilder insertSql = new StringBuilder("INSERT INTO FORUM_TOPICS ");
            insertSql.Append("(CAT_ID,FORUM_ID,T_STATUS,T_SUBJECT,T_MESSAGE,T_AUTHOR,T_LAST_POST_AUTHOR,T_REPLIES,T_VIEW_COUNT,T_DATE,T_LAST_POST,T_IP,T_STICKY,T_SIG,T_UREPLIES) VALUES ( ");
            insertSql.AppendLine("@CatId,");
            insertSql.AppendLine("@ForumId,");
            insertSql.AppendLine("@Status,");
            insertSql.AppendLine("@Subject,");
            insertSql.AppendLine("@Message,");
            insertSql.AppendLine("@AuthorId,");
            insertSql.AppendLine("@AuthorId,");
            insertSql.AppendLine("0,0,"); //reply + view count
            insertSql.AppendLine("@Date,");
            insertSql.AppendLine("@Date,");
            insertSql.AppendLine("@PostersIP,");
            insertSql.AppendLine("@Sticky,");
            insertSql.AppendLine("@UseSig,");
            insertSql.AppendLine("0 );"); //unmoderated post count
            insertSql.AppendLine(" SELECT SCOPE_IDENTITY();"); //Get the inserted topic Id

            List<SqlParameter> parms = new List<SqlParameter>
                {
                    new SqlParameter("@CatId", SqlDbType.Int) {Value = topic.CatId},
                    new SqlParameter("@ForumId", SqlDbType.Int) {Value = topic.ForumId},
                    new SqlParameter("@Status", SqlDbType.Int) {Value = topic.Status},
                    new SqlParameter("@Subject", SqlDbType.NVarChar) {Value = topic.Subject},
                    new SqlParameter("@Message", SqlDbType.NVarChar) {Value = topic.Message},
                    new SqlParameter("@AuthorId", SqlDbType.Int) {Value = topic.AuthorId},
                    new SqlParameter("@Date", SqlDbType.VarChar) {Value = topic.Date.ToString("yyyyMMddHHmmss")},
                    new SqlParameter("@PostersIP", SqlDbType.VarChar) {Value = topic.PosterIp},
                    new SqlParameter("@Sticky", SqlDbType.Bit) {Value = topic.IsSticky},
                    new SqlParameter("@UseSig", SqlDbType.Bit) {Value = topic.UseSignatures}
                };

            int topicid = Convert.ToInt32(SqlHelper.ExecuteScalar(SqlHelper.ConnString, CommandType.Text, insertSql.ToString(), parms.ToArray()));
            topic.Id = topicid;
            new Forum().UpdateLastForumPost(topic);
            new Member().UpdateLastMemberPost(topic);
            return topicid;
        }

        public void Update(TopicInfo topic)
        {
            List<SqlParameter> parms = new List<SqlParameter>
                {
                    
                    new SqlParameter("@Status", SqlDbType.Int) {Value = topic.Status},
                    new SqlParameter("@Subject", SqlDbType.NVarChar) {Value = topic.Subject},
                    new SqlParameter("@Message", SqlDbType.NVarChar) {Value = topic.Message},
                    new SqlParameter("@Sticky", SqlDbType.Bit) {Value = topic.IsSticky},
                    new SqlParameter("@UseSig", SqlDbType.Bit) {Value = topic.UseSignatures},
                    new SqlParameter("@TopicId", SqlDbType.Int) {Value = topic.Id}
                };

            StringBuilder updateSql = new StringBuilder("UPDATE FORUM_TOPICS SET ");
            updateSql.AppendLine("T_STATUS=@Status,");
            updateSql.AppendLine("T_SUBJECT=@Subject,");
            updateSql.AppendLine("T_MESSAGE=@Message,");
            if (topic.LastEditedById.HasValue)
            {
                updateSql.AppendLine("T_LAST_EDITBY=@EditedBy,");
                updateSql.AppendLine("T_LAST_EDIT=@EditDate,");
                parms.Add(new SqlParameter("@EditedBy", SqlDbType.Int) { Value = topic.LastEditedById.Value });
                parms.Add(new SqlParameter("@EditDate", SqlDbType.VarChar) { Value = DateTime.UtcNow.ToString("yyyyMMddHHmmss") });

            }
            updateSql.AppendLine("T_STICKY=@Sticky,");
            updateSql.AppendLine("T_SIG=@UseSig ");
            updateSql.AppendLine("WHERE TOPIC_ID=@TopicId");

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, updateSql.ToString(), parms.ToArray());

        }

        public void Delete(TopicInfo topic)
        {
            const string strSql = "DELETE FROM FORUM_REPLY WHERE TOPIC_ID=@TopicId; DELETE FROM FORUM_TOPICS WHERE TOPIC_ID=@TopicId;";

            SqlHelper.ExecuteNonQuery(SqlHelper.ConnString, CommandType.Text, strSql, new SqlParameter("@TopicId", SqlDbType.Int) { Value = topic.Id });
            new AdminFunctions().UpdateForumCounts();
        }

        public void Dispose()
        {

        }

        #endregion

    }
}
