using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.OleDb;

namespace DataAccess
{
  /// <summary>
  /// AccessHelper �����߼����ʰ�����
  /// update 2004-10-07
  /// </summary>
  public class AccessHelper
  {
    /// <summary>
    /// ���ݿ����Ӵ�
    /// </summary>  
    //public static string CONN_STRING_ACCESS = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + AppDomain.CurrentDomain.BaseDirectory.ToString() + ConfigurationSettings.AppSettings["CONNSTR_Access"];
	  public static string CONN_STRING_ACCESS = GetConnStr();

    //public static readonly string CONN_STRING_SQL = ConfigurationSettings.AppSettings["CONNSTR_SQL"];

    // �洢�����Ĺ�ϣ��
    private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

	public AccessHelper()
  	{

	}

	public static string GetConnStr()
	  {
		  string conn = "";
		  //��ת����Сд,�������ִ�Сд
          //string strTemp = ConfigurationManager.AppSettings["Webdiy_DBType"].ToLower();
          //if (strTemp == "access")
          conn = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + AppDomain.CurrentDomain.BaseDirectory.ToString() + ConfigurationManager.AppSettings["MsnRobotDbPath"];
          //else if (strTemp == "sqlserver")
          //    conn = "Provider=sqloledb;" + ConfigurationManager.AppSettings["CONNSTR_SQL"];
          //    //throw new Exception("���˰治֧��sql���ݿ�!");
          //else
          //    throw new Exception("�����ļ�����!");

		  return conn;
	  }

    /// <summary>
    /// ִ��һ��û�з������ݽ������SQL����洢����
    /// </summary>
    /// <param name="connString">��Ч�����ݿ����Ӵ�</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>Ӱ�������</returns>
    public static int ExecuteNonQuery(string connString, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();

      // �� using ��Χ���Զ����� conn ����
      using (OleDbConnection conn = new OleDbConnection(connString))
      {
        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        int val = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        return val;
      }
    }

    /// <summary>
    /// ִ��һ��û�з������ݽ������SQL����洢����
    /// </summary>
    /// <param name="conn">���ݿ����Ӷ���</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>Ӱ�������</returns>
    public static int ExecuteNonQuery(OleDbConnection conn, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();

      PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
      int val = cmd.ExecuteNonQuery();
      cmd.Parameters.Clear();
      return val;
    }

    /// <summary>
    /// ִ��һ��û�з������ݽ������SQL����洢����
    /// </summary>
    /// <param name="trans">���ڵ�����</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>Ӱ�������</returns>
    public static int ExecuteNonQuery(OleDbTransaction trans, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();

      PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, cmdParms);
      int val = cmd.ExecuteNonQuery();
      cmd.Parameters.Clear();
      return val;
    }

    /// <summary>
    /// ִ��һ��SQL������� OleDbDataReader
    /// </summary>
    /// <param name="connString">���ݿ����Ӵ�</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>Ӱ�������</returns>
    public static OleDbDataReader ExecuteReader(string connString, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();
      OleDbConnection conn = new OleDbConnection(connString);

      // ������ try/catch ������Ҫ�ǿ��ǵ�����SQL����ִ���쳣ʱ��û�н��� OleDbDataReader ����
      // ��ȻҲ���޷�ʹ�� CommandBehavior.CloseConnection ���ر����ݿ����Ӷ��� conn
      // ��ˣ������� try/catch ���ƣ����쳣ʱ���йر����ݿ����Ӷ��� conn
      try
      {
        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        OleDbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
        cmd.Parameters.Clear();
        return rdr;
      }
      catch
      {
        conn.Close();
        throw;
      }
    }


    /// <summary>
    /// ִ��һ�� SQL �������һ����Ԫ���ֵ
    /// </summary>
    /// <param name="connString">���ݿ����Ӵ�</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>object</returns>
    public static object ExecuteScalar(string connString, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();

      using (OleDbConnection conn = new OleDbConnection(connString))
      {
        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        object val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
      }
    }

    /// <summary>
    /// ִ��һ�� SQL �������һ����Ԫ���ֵ
    /// </summary>
    /// <param name="conn">���ݿ����Ӷ���</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>object</returns>
    public static object ExecuteScalar(OleDbConnection conn, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();

      PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
      object val = cmd.ExecuteScalar();
      cmd.Parameters.Clear();
      return val;
    }

    /// <summary>
    /// ִ��һ�� SQL �������ȡ�õ����ݼ�
    /// </summary>
    /// <param name="connString">���ݿ����Ӵ�</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>������ݼ�</returns>
    public static DataSet ExecuteDataset(string connString, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();
      OleDbConnection conn = new OleDbConnection(connString);
      DataSet ds = new DataSet();

      try
      {
        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        OleDbDataAdapter da = new OleDbDataAdapter();
        da.SelectCommand = cmd;
        da.Fill(ds);
        return ds;
      }
      catch
      {
        throw;
      }
      finally
      {
        conn.Close();
      }
    }

    /// <summary>
    /// ִ��һ�� SQL �������ȡ�õ����ݼ�
    /// </summary>
    /// <param name="conn">���ݿ����Ӷ���</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">ִ����Ҫ�Ĳ�������</param>
    /// <returns>������ݼ�</returns>
    public static DataSet ExecuteDataset(OleDbConnection conn, CommandType cmdType, string cmdText, params OleDbParameter[] cmdParms)
    {
      OleDbCommand cmd = new OleDbCommand();
      DataSet ds = new DataSet();

      try
      {
        PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParms);
        OleDbDataAdapter da = new OleDbDataAdapter();
        da.SelectCommand = cmd;
        da.Fill(ds);
        return ds;
      }
      catch
      {
        throw;
      }
      finally
      {
        conn.Close();
      }
    }

    /// <summary>
    /// ��������������ϣ��
    /// </summary>
    /// <param name="cacheKey">Key</param>
    /// <param name="cmdParms">Ҫ���������</param>
    public static void CacheParameters(string cacheKey, params OleDbParameter[] cmdParms)
    {
      parmCache[cacheKey] = cmdParms;
    }

    /// <summary>
    /// ȡ�ù�ϣ����Ĳ���
    /// </summary>
    /// <param name="cacheKey">Key</param>
    /// <returns>��������ֵ</returns>
    public static OleDbParameter[] GetCachedParameters(string cacheKey)
    {
      OleDbParameter[] cachedParms = (OleDbParameter[]) parmCache[cacheKey];

      if (cachedParms == null)
        return null;

      OleDbParameter[] clonedParms = new OleDbParameter[cachedParms.Length];

      for (int i = 0, j = cachedParms.Length; i < j; i++)
        clonedParms[i] = (OleDbParameter) ((ICloneable) cachedParms[i]).Clone();

      return clonedParms;
    }

    /// <summary>
    /// �Բ����������׼��
    /// </summary>
    /// <param name="cmd">OleDbCommand ����</param>
    /// <param name="conn">OleDbConnection ����</param>
    /// <param name="trans">OleDbTransaction ����</param>
    /// <param name="cmdType">SQL�������ͣ�SQL����洢����</param>
    /// <param name="cmdText">�����ַ���</param>
    /// <param name="cmdParms">��������</param>
    private static void PrepareCommand(OleDbCommand cmd, OleDbConnection conn, OleDbTransaction trans, CommandType cmdType, string cmdText, OleDbParameter[] cmdParms)
    {
      if (conn.State != ConnectionState.Open)
        conn.Open();

      cmd.Connection = conn;
      cmd.CommandText = cmdText;

      if (trans != null)
        cmd.Transaction = trans;

      cmd.CommandType = cmdType;

      if (cmdParms != null)
      {
        foreach (OleDbParameter parm in cmdParms)
          cmd.Parameters.Add(parm);
      }
    }

    #region �����ӵ����ط���
    //**************************�����Ĳ��ֿ�ʼ***********************************
    /// <summary>
    /// ִ��һ�� SQL �������DataSet
    /// </summary>
    /// <param name="cmdText"></param>
    /// <returns></returns>
    public static DataSet ExecuteDataset(string cmdText)
    {
      OleDbCommand cmd = new OleDbCommand();
      OleDbConnection conn = new OleDbConnection(AccessHelper.CONN_STRING_ACCESS);
      DataSet ds = new DataSet();

      try
      {
        if (conn.State != ConnectionState.Open)
          conn.Open();

        cmd.Connection = conn;
        cmd.CommandText = cmdText;
        cmd.CommandType = CommandType.Text;

        OleDbDataAdapter da = new OleDbDataAdapter();
        da.SelectCommand = cmd;
        da.Fill(ds);
        return ds;
      }
      catch
      {
        throw;
      }
      finally
      {
        conn.Close();
      }
    }

    /// <summary>
    /// ִ��һ�� SQL �������DataTable
    /// </summary>
    /// <param name="cmdText"></param>
    /// <returns></returns>
    public static DataTable ExecuteDataTable(string cmdText)
    {
      DataSet ds = ExecuteDataset(cmdText);
      if (ds.Tables.Count > 0)
        return ds.Tables[0];
      else
        return null;
    }

    /// <summary>
    /// ִ��һ�� SQL �������DataRow
    /// </summary>
    /// <param name="cmdText"></param>
    /// <returns></returns>
    public static DataRow ExecuteDataRow(string cmdText)
    {
      DataSet ds = ExecuteDataset(cmdText);
      if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        return ds.Tables[0].Rows[0];
      else
        return null;
    }

    /// <summary>
    /// ִ��һ�� SQL ���������Ӱ�������
    /// </summary>
    /// <param name="cmdText"></param>
    /// <returns></returns>
    public static int ExecuteNonQuery(string cmdText)
    {
      OleDbCommand cmd = new OleDbCommand();

      // �� using ��Χ���Զ����� conn ����
      using (OleDbConnection conn = new OleDbConnection(AccessHelper.CONN_STRING_ACCESS))
      {
        PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, null);
        int val = cmd.ExecuteNonQuery();
        cmd.Parameters.Clear();
        return val;
      }
    }

    /// <summary>
    /// ִ��һ�� SQL ������ص�һ��,��һ��
    /// </summary>
    /// <param name="cmdText"></param>
    /// <returns></returns>
    public static object ExecuteScalar(string cmdText)
    {
      OleDbCommand cmd = new OleDbCommand();

      using (OleDbConnection conn = new OleDbConnection(AccessHelper.CONN_STRING_ACCESS))
      {
        PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, null);
        object val = cmd.ExecuteScalar();
        cmd.Parameters.Clear();
        return val;
      }
    }

      public static OleDbDataReader ExecuteReader(string cmdText)
      {
          OleDbCommand cmd = new OleDbCommand();
          OleDbConnection conn = new OleDbConnection(AccessHelper.CONN_STRING_ACCESS);
          try
          {
              PrepareCommand(cmd, conn, null, CommandType.Text, cmdText, null);
              OleDbDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
              cmd.Parameters.Clear();
              return rdr;
          }
          catch
          {
              conn.Close();
              throw;
          }
      }
   //**************************�����Ĳ��ֽ���***********************************
      #endregion
  }
}