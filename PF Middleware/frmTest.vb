Imports System.Data.Odbc
Imports System.Data.SqlClient
Imports System.Threading

Public Class frmTest
    Dim STime As DateTime
    Dim ETimeLIS As DateTime
    Dim ETimePOS As DateTime
    Dim ELog3 As DateTime
    Dim ELog5 As DateTime
    Dim ETime As DateTime
    Dim MoveForm As Boolean
    Dim MoveForm_MousePosition As Point
    Dim vSuccess As String = ""
    Dim ctr As Integer = 0
    Public vDate As Date
    Dim sqlconn As New SQLCon.HRSrvr

    Private Sub btnProcess_Click(sender As Object, e As EventArgs) Handles btnProcess.Click
        PBImportInv.Visible = True
        PBImportInv.BackColor = Color.Transparent
        CheckForIllegalCrossThreadCalls = False
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Private Sub ImportDoctorsClinic()
        Dim cnMain As New SqlConnection("Data Source=192.171.3.29;Initial Catalog=DRClinics;Integrated Security=False;" & _
                                        "UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;")
        If cnMain.State = ConnectionState.Closed Then cnMain.Open()

        Dim cn As New SqlConnection("Data Source=192.171.3.29;Initial Catalog=DRCLINICS;Integrated Security=False;" & _
                                    "UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;")
        If cn.State = ConnectionState.Closed Then cn.Open()

        Dim str As String = "Truncate table DRCLINICS..ConsolUserMasterTemp"
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select * From DRClinics..SAPSet Where Stat = 'O'"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader

        If dr.HasRows Then
            While dr.Read
                Try
                    Dim cnBr As New SqlConnection("Data Source=" & dr("Srce") & ";Initial Catalog=DRClinics;Integrated Security=False;" & _
                                              "UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;")
                    If cn.State = ConnectionState.Closed Then cn.Open()
                    str = "Select PhysicianID,RPhysicianName,UserID,Username,[Password],'" & dr("Code") & "',isnull(EmpID,'')EmpID,stat " & _
                          "From DRCLINICS..User_Master o " & _
                          "Inner Join DRCLINICS..PHYSICIANS i on o.PhysicianID = i.RPhysicianID " & _
                          "where AccessLevel = 'Doctor'"
                    Using dt2 As New DataTable("User_Master")
                        Try
                            Using da2 As New SqlDataAdapter(str, cnBr)
                                da2.Fill(dt2)
                            End Using
                            Using bulkCopy As New SqlBulkCopy(cnMain)
                                bulkCopy.DestinationTableName = "ConsolUserMasterTemp"
                                bulkCopy.BulkCopyTimeout = 0
                                bulkCopy.WriteToServer(dt2)
                                bulkCopy.Close()
                            End Using
                        Catch ex As Exception
                        Finally
                        End Try
                    End Using
                Catch ex As Exception
                    Continue While
                End Try
            End While
        End If

        str = "Insert into ConsolUserMaster " & _
              "Select distinct o.* " & _
              "From ConsolUserMasterTemp o With (Nolock) " & _
              "Left join ConsolUserMaster i With (Nolock) on o.whscode = i.whscode and o.PhysicianID = i.PhysicianID " & _
              "Where isnull(i.PhysicianID,'') = '' "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

    End Sub

    Private Sub ProcessImport()
        lblTime.Text = "Stablishing connection please wait ..."
        Dim cn As New SqlConnection()
        cn = New SqlConnection("Data Source=" & SAPSrvr & ";Initial Catalog=" & DbCommon & ";Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                               "Application Name = " & My.Application.Info.AssemblyName)
        If cn.State = ConnectionState.Closed Then cn.Open()
        Dim DTNow As DateTime = Format(Now(), "MM/dd/yyyy 18:00:00")
        Dim DTNow2 As DateTime = Format(DateAdd(DateInterval.Day, 1, Now()), "MM/dd/yyyy 05:00:00")
        If Now() >= DTNow And Now <= DTNow2 Then
            lblTime.Text = "Importing doctor's clinic doctor please wait ..."
            Try
                ImportDoctorsClinic()
            Catch ex As Exception
            End Try
        End If

        lblTime.Text = "Update doctor's start date process on going, Please wait ..."
        Dim str As String = "UPDATE dbo.PFDoctors SET DateStarted = d.DateStarted " & _
                            "FROM [192.171.10.51].hpcommon.dbo.scdr d WITH (NOLOCK) " & _
                            "WHERE d.DrCode = dbo.PFDoctors.DSCode COLLATE DATABASE_DEFAULT "
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        lblTime.Text = "Import invoice and adjustment please wait ..."
        str = "Exec InvImport"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        cn.Dispose()
        cn = New SqlConnection("Data Source=" & SAPSrvr & ";Initial Catalog=" & DbCommon & ";Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                               "Application Name = " & My.Application.Info.AssemblyName)
        If cn.State = ConnectionState.Closed Then cn.Open()


        str = "Select ISNULL(max(docEntry),0)+1 from ImpLogs"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        str = cmd.ExecuteScalar

        str = "Insert into ImpLogs(docEntry,docdate,ipadd,empid,whscode ) " & _
              "Values('" & str & "',getdate(),'MIDDLEWARE','MIDDLEWARE','ALL' )"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select GetDate()"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim SDate As DateTime = cmd.ExecuteScalar
        Dim EDate As DateTime = SDate

        str = "Select Blk,Code from HPCOMMON..SAPSET WHERE slsstat = 'O' and IsNull(IMG_IPAdd,'') <> '' "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader
        If dr.HasRows Then
            While dr.Read
                Dim code As String = dr("Code")
                lblTime.Text = "Imaging Import -- Importing " & dr("Blk")
                SDate = Format(SDate, "MM/dd/yyyy")
                str = "ImgImport '" & DateAdd(DateInterval.Month, -1, SDate) & "', '" & Format(SDate, "MM/dd/yyyy") & "','" & dr("Code") & "' "
                cmd = New SqlCommand(str, cn)
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
            End While
        End If
        dr.Close()

        cn.Dispose()
        cn = New SqlConnection("Data Source=" & SAPSrvr & ";Initial Catalog=" & DbCommon & ";Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                             "Application Name = " & My.Application.Info.AssemblyName)
        If cn.State = ConnectionState.Closed Then cn.Open()

        lblTime.Text = "Tagging SAPCode and Result date to transaction ..."
        str = "Exec PF..PFHchy"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        'If chkOverride.Checked = True Then
        '    lblTime.Text = "Override PF_Regular transaction ..."
        '    str = "Update PF..PF Set Computation = Null Where Computation = 'PF_Regular' and isnull(Genid,'') = '' " & _
        '          "Update PF..PF Set Computation1 = Null Where Computation1 = 'PF_Regular' and isnull(Genid1,'') = '' " & _
        '          "Update PF..PF Set Computation2 = Null Where Computation2 = 'PF_Regular' and isnull(Genid2,'') = '' " & _
        '          "Update PF..PFDRC Set Computation = Null Where Computation = 'PF_Regular' and isnull(Genid,'') = '' "
        '    cmd = New SqlCommand(str, cn)
        '    cmd.CommandTimeout = 0
        '    cmd.ExecuteNonQuery()
        'End If
        'Update Technician Computation
        'str = "UPDATE PF..PF SET Computation1=NULL " & _
        '      "FROM (	SELECT o.docentry,o.LineNUm,o.ObjType " & _
        '      "		    FROM PF..PF o " & _
        '      "		    Inner Join (Select Distinct i.DSCode,i.SName,DCode,BID,o.DName,FtPt " & _
        '      "					    From PfDocdet o " & _
        '      "					    Inner Join PFDoctors i on o.DocCode = i.DsCode " & _
        '      "					    Inner Join pfrtdet d on i.SName = d.TechCode " & _
        '      "					    Where PFType = 'Technician') a on o.SAPCode1 = a.SName and o.Whscode = BID AND o.RCode = a.DCode AND o.RName = a.DName " & _
        '      "		    WHERE Computation1 IN ('PF_Technician','PF_Technician_OffDuty') AND ISNULL(GenID1,'') = '' AND isnull(a.FtPt,0) <> 0  ) a " & _
        '      "WHERE pf.docentry = a.docentry AND pf.LineNUm = a.LineNUm AND pf.ObjType = a.ObjType"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        str = "Select nopos,hcode From PF..PFHrchyTag WIth (Nolock) order by Cast(nopos as Int)"
        cmd = New SqlCommand(str, cn)
        dr = cmd.ExecuteReader
        If dr.HasRows Then
            While dr.Read
                lblTime.Text = "Updating " & dr("HCode") & " Computation ..."
                str = "Exec " & dr("HCode") & " '" & dr("HCode") & "','','' "
                cmd = New SqlCommand(str, cn)
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
            End While

            lblTime.Text = "Updating Technician Computation OFF Duty..."
            UpdateTechNicianRatesOffDuty(cn, cmd)
            UpdateTechNicianRatesOffDutySpecialCase(cn, cmd)
            'UpdateTechNicianRates_Oncall(cn, cmd)

            lblTime.Text = "Updating Regular Computation ..."
            str = "Exec PF_Regular 'PF_Regular','','' "
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        End If

        'lblTime.Text = "Truncating doctor ..."
        'str = "Truncate Table SCDr"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        'lblTime.Text = "Inserting doctor ..."
        'str = "Insert into SCDr " & _
        '      "Select DrCode,DrName,DateStarted,BranchCode From [192.171.11.7].HPCOMMON.DBO.SCDr"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        str = "Update HPCOMMON..SAPOPT set PFMW = GetDate()"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()   

        GetLastUpd()
        cmd.Dispose()
        cn.Dispose()
    End Sub

    Private Sub ProcessImport_SendOut()
        Try
            lblTime.Text = "Stablishing connection please wait ..."
            Dim cn As New SqlConnection()
            cn = New SqlConnection("Data Source=" & SAPSrvr & ";Initial Catalog=" & DbCommon & ";Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                                   "Application Name = " & My.Application.Info.AssemblyName)
            If cn.State = ConnectionState.Closed Then cn.Open()
            Dim cmd As SqlCommand
            Dim str As String = ""

            'Import Sendout Data from 11.106
            lblTime.Text = "Import Sendout Data"
            BulkInsert()

            Try
                lblTime.Text = "Import send out invoice(s) process on going, Please wait ..."
                cmd = New SqlCommand("Exec SO_InvImport", cn)
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
            Catch ex As Exception
            End Try

            Dim cnPFF As New SqlConnection("Data Source=" & SAPSrvr & ";Initial Catalog=" & DbCommon & ";Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                                           "Application Name = " & My.Application.Info.AssemblyName)
            If cnPFF.State = ConnectionState.Closed Then cnPFF.Open()

            Try
                lblTime.Text = "Updating send out invoice(s) result please wait ..."
                cmd = New SqlCommand("Exec SO_UpdResult", cnPFF)
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
            Catch ex As Exception
            End Try

            lblTime.Text = "Updating send out histopath result please wait ..."
            str = "Update PF..SO Set ResultDate = o.ReceiveDate,Authorized = 'A',Remarks = o.Diagnosis " & _
                  "From HPCOMMON..HistopathTAT o With (Nolock) " & _
                  "Where o.TrxStatus in ('2','3','4') and SO.u_labno = o.TrxNo and SO.itemcode = o.ExamID and isnull(Authorized,'') = ''"
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()

            str = "Update PF..SOAdj Set Status = 'APPROVED',Reason_Dec = 'AUTO APPROVED BY SYSTEM' " & _
                  "FROM (	SELECT o.DocEntry,i.DocEntry InvEntry,i.LineNUm,i.ObjType,i.ResultDate,o.U_LabNo,o.ItemCode " & _
                  "		    FROM SOAdj o WITH (NOLOCK) " & _
                  "		    INNER JOIN pf..SO i WITH (NOLOCK) ON o.U_LabNo = i.u_labno AND o.ItemCode = i.itemcode " & _
                  "		    WHERE AdjType = 'NR' AND o.[Status] = 'FOR CHECKING' AND i.ResultDate IS NOT NULL ) a " & _
                  "WHERE SOAdj.DocEntry = a.DocEntry"
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()

            cmd.Dispose()
            cn.Dispose()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub AutoAppSched()
        lblTime.Text = "Processing schedule auto approve..."
        Dim cn As New SqlConnection("Data Source=192.171.10.51;Initial Catalog=HPCOMMON;Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                                    "Application Name = " & My.Application.Info.AssemblyName)
        If cn.State = ConnectionState.Closed Then cn.Open()

        'Holiday
        Dim str As String = "Select Top 1 1 From Calendar Where Yr >= Year(GetDate()) and isnull(Typ,'') <> '' and Cast(Dt as date) = Cast(GetDate() as date)"
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        str = cmd.ExecuteScalar
        If Not str Is Nothing Then
            str = "Update HPCOMMON..PFRlvrDtl Set Stat = 1,AppBy=a.PFOfcr,AppDate=GetDate() " & _
                  "From (	Select o.DocDate,o.DocEntry,o.WhsCode,s.PFOfcr " & _
                  "         From HPCOMMON..PFRlvrDtl o With (Nolock) " & _
                  "         Inner Join HPCOMMON..SAPSet s With (Nolock) on o.WhsCode = s.Code " & _
                  "         Where o.DocDate = cast(GetDate() as date) and isnull(o.Stat,'') = '' ) a " & _
                  "         Where PFRlvrDtl.DocEntry=a.DocEntry and PFRlvrDtl.DocDate=a.DocDate and PFRlvrDtl.WhsCode=a.WhsCode "
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        End If

        'Saturday and Sunday
        str = "Select Top 1 1 From Calendar Where Yr >= Year(GetDate()) and Dy In ('Saturday','Sunday') and cast(Dt as date)= Cast(GetDate() as date)"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        str = cmd.ExecuteScalar
        If Not str Is Nothing Then
            str = "Update HPCOMMON..PFRlvrDtl Set Stat = 1,AppBy=a.PFOfcr,AppDate=GetDate() " & _
                  "From (	Select o.DocDate,o.DocEntry,o.WhsCode,s.PFOfcr " & _
                  "         From HPCOMMON..PFRlvrDtl o With (Nolock) " & _
                  "         Inner Join HPCOMMON..SAPSet s With (Nolock) on o.WhsCode = s.Code " & _
                  "         Where o.DocDate = cast(GetDate() as date) and isnull(o.Stat,'') = '' ) a " & _
                  "         Where PFRlvrDtl.DocEntry=a.DocEntry and PFRlvrDtl.DocDate=a.DocDate and PFRlvrDtl.WhsCode=a.WhsCode "
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        End If

        ''Auto approve due to ncov
        'str = "Select Top 1 1 From Calendar Where Yr >= Year(GetDate()) and Cast(Dt as date) = Cast(GetDate() as date)"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'str = cmd.ExecuteScalar
        'If Not str Is Nothing Then
        '    str = "Update HPCOMMON..PFRlvrDtl Set Stat = 1,AppBy=a.PFOfcr,AppDate=GetDate() " & _
        '          "From (	Select o.DocDate,o.DocEntry,o.WhsCode,s.PFOfcr " & _
        '          "         From HPCOMMON..PFRlvrDtl o With (Nolock) " & _
        '          "         Inner Join HPCOMMON..SAPSet s With (Nolock) on o.WhsCode = s.Code " & _
        '          "         Where o.DocDate = cast(GetDate() as date) and isnull(o.Stat,'') = '' ) a " & _
        '          "         Where PFRlvrDtl.DocEntry=a.DocEntry and PFRlvrDtl.DocDate=a.DocDate and PFRlvrDtl.WhsCode=a.WhsCode "
        '    cmd = New SqlCommand(str, cn)
        '    cmd.CommandTimeout = 0
        '    cmd.ExecuteNonQuery()
        'End If


        cmd.Dispose()
        cn.Dispose()
    End Sub

    Private Sub BulkInsert()
        Dim cn As New SqlConnection("Data Source=192.171.11.123;Initial Catalog=LabMerieux;Integrated Security=False;UID=sapdb;PWD=sapdb;")
        If cn.State = ConnectionState.Closed Then cn.Open()

        Dim cnPF As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb;")
        If cnPF.State = ConnectionState.Closed Then cnPF.Open()

        Dim str As String = "Truncate table SO_Master_PF"
        Dim cmd As New SqlCommand(str, cnPF)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select LabNo,PatientName,Age,SendOutLab,TestRequested,TestCode,SendOutDate,DateCreated,[Status],VenCost,PricConv, " & _
              "       CurrCode,Remarks,ResultDate,Airwaybill,DCode,MacroPrice,SourceProg " & _
              "From SO_Master With (Nolock) " & _
              "Where Cast(SendOutDate as date) >= DateAdd(M,-12,Cast(GetDate() as date))"
        Using dt2 As New DataTable("SO_Master")
            Using da2 As New SqlDataAdapter(str, cn)
                da2.Fill(dt2)
            End Using
            Using bulkCopy As New SqlBulkCopy("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb;")
                bulkCopy.DestinationTableName = "SO_Master_PF"
                Try
                    'Write from the source to the destination.
                    bulkCopy.BulkCopyTimeout = 0
                    bulkCopy.WriteToServer(dt2)
                    bulkCopy.Close()
                Catch ex As Exception
                Finally
                End Try
            End Using
        End Using
    End Sub

    Public Sub UpdateTechNicianRatesOffDuty(ByVal cn As SqlConnection, cmd As SqlCommand)
        Dim str As String = ""
        If cn.State = ConnectionState.Closed Then cn.Open()
        Try
            str = "Drop table #Doctor"
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        Catch ex As Exception : End Try

        Try
            str = "Drop table #tmp"
            cmd = New SqlCommand(str, cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        Catch ex As Exception : End Try

        str = "CREATE TABLE #SCList (Code VARCHAR(30),DocDate Date,WhsCode VARCHAR(10))"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()


        str = "Select Distinct DocCode " & _
              "From PFDocdet o With (Nolock) " & _
              "Inner Join PFDoctors i With (Nolock) on o.DocCode = i.DSCode " & _
              "Inner Join pfrtdet d on i.SName = d.TechCode " & _
              "Where PFType = 'Technician' and isnull(d.FtPt,0) <> 0 "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim Tech As String = ""
        Dim drTech As SqlDataReader = cmd.ExecuteReader
        If drTech.HasRows Then
            While drTech.Read
                If Tech = "" Then
                    Tech = "'" & drTech("DocCode") & "'"
                Else
                    Tech = Tech & ",'" & drTech("DocCode") & "'"
                End If
            End While
            Tech = " and Code in (" & Tech & ")"
        End If
        drTech.Close()

        Dim cnHPL As New SqlConnection("Data Source=192.171.10.51;Initial Catalog=HPCOMMON;Integrated Security=False;" & _
                                        "UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;")
        If cnHPL.State = ConnectionState.Closed Then cnHPL.Open()

        str = "Select Code,DocDate,WhsCode " & _
              "From HPCOMMON.DBO.SCList With (Nolock) " & _
              "Where Docdate Between Dateadd(month,-3,cast(GetDate() as date)) and cast(GetDate() as date) and Leg In ('Off','No Duty') " & Tech
        Using dt2 As New DataTable("SCList")
            Try
                Using da2 As New SqlDataAdapter(str, cnHPL)
                    da2.Fill(dt2)
                End Using
                Using bulkCopy As New SqlBulkCopy(cn)
                    bulkCopy.DestinationTableName = "#SCList"
                    bulkCopy.BulkCopyTimeout = 0
                    bulkCopy.WriteToServer(dt2)
                    bulkCopy.Close()
                End Using
            Catch ex As Exception
            Finally
            End Try
        End Using

        str = "Select Distinct o.DSCode,SName,BID " & _
              "Into #Doctor " & _
              "From PF..PFDOCDET i With (Nolock) " & _
              "INNER jOIN PF..PFDOCTORS o With (Nolock) on i.DocCode = o.DSCode " & _
              "Where DType = 'Technician'"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded)ProcDate,Sum(p.LineTotal)LineTotal, " & _
              "     p.RRate,p.RAmtRate,Sum(p.RPF)RPF,Computation1, " & _
              "		ROW_NUMBER() over(PARTITION BY p.sapcode1,CONVERT(NVARCHAR(25),ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded), 111) ORDER BY ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded) ASC) AS Number " & _
              "Into #tmp " & _
              "From PF..PF p With (Nolock) " & _
              "Left Join ImagingDoctor i With (Nolock) on left(p.U_Labno,10) = i.img_trxno and p.WhsCode = i.IMG_BRANCH and p.ItemCode = i.IMG_EXAM " & _
              "Left Join HPCOMMON..ImgTAT tat With (Nolock) on left(p.U_Labno,10) = tat.TrxNo and p.WhsCode = tat.WhsCode and p.ItemCode = tat.PendingExam " & _
              "Inner Join ( Select Distinct DocDate,Code,SName,DCode,i.BID " & _
              "			    From #SCList o With (Nolock) " & _
              "			    Inner Join #Doctor i With (Nolock) on o.Code = i.DSCode Collate database_default and o.WhsCode = i.BID Collate database_default " & _
              "			    Inner Join PF..PFDocDet d With (Nolock) on i.DSCode = d.DocCode) d on ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded)= d.DocDate and p.RCode = d.DCode and p.whscode = d.BID and p.SAPCode1 = d.SName " & _
              "Inner Join pfRtRates r With (Nolock) on p.itemcode = r.itemcode and p.whscode = r.whscode and p.SAPCode1 = r.TechCode " & _
              "Where IsNull(GenID1,'') = '' and IsNull(Computation1,'') in ('','PF_Technician') and p.SAPCode1 <> 'MARY ANGELINE PARIÑAS'  " & _
              "Group By p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded),p.RRate,p.RAmtRate,Computation1 " & _
              "Having Sum(p.RPF) <> 0 " & _
              "Order By p.whscode,p.SAPCode1,p.docdate,p.u_labno "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select o.*,Case when o.DocDate >= i.EffDate Then i.FtPt Else i.PrevFtPt End FtPt, " & _
              "         Case when o.DocDate >= i.EffDate Then i.SuccPt Else i.PrevSuccPt End SuccPt " & _
              "From #tmp o With (Nolock) " & _
              "Inner Join pfrtdet i With (Nolock) on o.sapcode1 = i.TechCode and o.whscode = i.Whscode " & _
              "WHere isnull(i.FtPt,0) <> 0 " & _
              "Order by o.whscode,SAPCode1,o.docdate,o.u_labno,Number"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader

        Dim WhsCode As String = ""
        Dim SAPCode1 As String = ""
        Dim ProcDate As String = ""
        Dim Number As String = ""

        Dim LabNo As String = ""
        Dim WhsCodeT As String = ""
        Dim ItemCode As String = ""

        If dr.HasRows Then
            While dr.Read
                Dim vChk As Boolean = False
                If WhsCode = "" Then WhsCode = dr("WhsCode")
                If SAPCode1 = "" Then SAPCode1 = dr("SAPCode1")
                If ProcDate = "" Then ProcDate = dr("ProcDate")
                If Number = "" Then Number = dr("Number")

                If WhsCode = dr("WhsCode") Then
                    If SAPCode1 = dr("SAPCode1") Then
                        If ProcDate = dr("ProcDate") Then
                            vChk = True
                        Else
                            vChk = False
                        End If
                    Else
                        vChk = False
                    End If
                Else
                    vChk = False
                End If

                If vChk = False Then
                    LabNo = ""
                    WhsCodeT = ""
                    ItemCode = ""
                    vChk = True
                End If

                If vChk = True Then
                    If dr("Number") > 3 Then
                        str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("SuccPt")) & "), " & _
                              "                     RAmtRate = " & CDbl(dr("SuccPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' " & _
                              "     and ISNULL(GENID1,'') = ''  "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()
                    Else
                        str = "Update PF..PF Set RPF = 0,RAmtRate = 0,Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & LabNo & "' and WhsCode = '" & WhsCodeT & "' and ItemCode = '" & ItemCode & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()

                        str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("FtPt")) & "), " & _
                              "                     RAmtRate = " & CDbl(dr("FtPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()
                    End If
                End If

                LabNo = dr("u_labno")
                WhsCodeT = dr("WhsCode")
                ItemCode = dr("itemcode")

                WhsCode = dr("WhsCode")
                SAPCode1 = dr("SAPCode1")
                ProcDate = dr("ProcDate")
            End While
        End If
        dr.Close()

        str = "Drop table #Doctor"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Drop table #tmp"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        'If cn.State = ConnectionState.Closed Then cn.Open()
        'Dim str As String = "Select Distinct DocCode " & _
        '                    "From PFDocdet o With (Nolock) " & _
        '                    "Inner Join PFDoctors i With (Nolock) on o.DocCode = i.DSCode " & _
        '                    "Inner Join pfrtdet d on i.SName = d.TechCode " & _
        '                    "Where PFType = 'Technician' and isnull(d.FtPt,0) <> 0 "
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'Dim Tech As String = ""
        'Dim drTech As SqlDataReader = cmd.ExecuteReader
        'If drTech.HasRows Then
        '    While drTech.Read
        '        If Tech = "" Then
        '            Tech = "'" & drTech("DocCode") & "'"
        '        Else
        '            Tech = Tech & ",'" & drTech("DocCode") & "'"
        '        End If
        '    End While
        '    Tech = " and Code in (" & Tech & ")"
        'End If
        'drTech.Close()

        'str = "Select * " & _
        '      "Into #SCList " & _
        '      "From [192.171.11.7].HPCOMMON.DBO.SCList With (Nolock) " & _
        '      "Where Docdate Between Dateadd(month,-3,cast(GetDate() as date)) and cast(GetDate() as date) and Leg In ('Off','No Duty') " & Tech
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        'str = "Select Distinct o.DSCode,SName,BID " & _
        '      "Into #Doctor " & _
        '      "From PF..PFDOCDET i With (Nolock) " & _
        '      "INNER jOIN PF..PFDOCTORS o With (Nolock) on i.DocCode = o.DSCode " & _
        '      "Where DType = 'Technician'"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        'str = "Select p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,cast(i.IMG_ENCODED_DATE as date)ProcDate,Sum(p.LineTotal)LineTotal, " & _
        '      "     p.RRate,p.RAmtRate,Sum(p.RPF)RPF,Computation1, " & _
        '      "		ROW_NUMBER() over(PARTITION BY p.sapcode1,CONVERT(NVARCHAR(25), cast(i.IMG_ENCODED_DATE as date), 111) ORDER BY cast(i.IMG_ENCODED_DATE as date) ASC) AS Number " & _
        '      "Into #tmp " & _
        '      "From PF..PF p With (Nolock) " & _
        '      "Inner Join ImagingDoctor i With (Nolock) on left(p.U_Labno,10) = i.img_trxno and p.WhsCode = i.IMG_BRANCH and p.ItemCode = i.IMG_EXAM " & _
        '      "Inner Join ( Select Distinct DocDate,Code,SName,DCode,i.BID " & _
        '      "			    From #SCList o " & _
        '      "			    Inner Join #Doctor i on o.Code = i.DSCode Collate database_default and o.WhsCode = i.BID Collate database_default " & _
        '      "			    Inner Join PF..PFDocDet d on i.DSCode = d.DocCode) d on cast(i.IMG_ENCODED_DATE as date)= d.DocDate and p.RCode = d.DCode and p.whscode = d.BID and p.SAPCode1 = d.SName " & _
        '      "Inner Join pfRtRates r With (Nolock) on p.itemcode = r.itemcode and p.whscode = r.whscode and p.SAPCode1 = r.TechCode " & _
        '      "Where IsNull(GenID1,'') = '' and IsNull(Computation1,'') in ('','PF_Technician') " & _
        '      "Group By p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,cast(i.IMG_ENCODED_DATE as date),p.RRate,p.RAmtRate,Computation1 " & _
        '      "Having Sum(p.RPF) <> 0 " & _
        '      "Order By p.whscode,p.SAPCode1,p.docdate,p.u_labno "
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        'str = "Select o.*,i.FtPt,i.SuccPt " & _
        '      "From #tmp o With (Nolock) " & _
        '      "Inner Join pfrtdet i With (Nolock) on o.sapcode1 = i.TechCode and o.whscode = i.Whscode " & _
        '      "Where isnull(i.FtPt,0) <> 0 " & _
        '      "Order by o.whscode,SAPCode1,o.docdate,o.u_labno,Number"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'Dim dr As SqlDataReader = cmd.ExecuteReader

        'Dim WhsCode As String = ""
        'Dim SAPCode1 As String = ""
        'Dim ProcDate As String = ""
        'Dim Number As String = ""

        'Dim LabNo As String = ""
        'Dim WhsCodeT As String = ""
        'Dim ItemCode As String = ""

        'If dr.HasRows Then
        '    While dr.Read
        '        Dim vChk As Boolean = False
        '        If WhsCode = "" Then WhsCode = dr("WhsCode")
        '        If SAPCode1 = "" Then SAPCode1 = dr("SAPCode1")
        '        If ProcDate = "" Then ProcDate = dr("ProcDate")
        '        If Number = "" Then Number = dr("Number")

        '        If WhsCode = dr("WhsCode") Then
        '            If SAPCode1 = dr("SAPCode1") Then
        '                If ProcDate = dr("ProcDate") Then
        '                    vChk = True
        '                Else
        '                    vChk = False
        '                End If
        '            Else
        '                vChk = False
        '            End If
        '        Else
        '            vChk = False
        '        End If

        '        If vChk = False Then
        '            LabNo = ""
        '            WhsCodeT = ""
        '            ItemCode = ""
        '            vChk = True
        '        End If

        '        If vChk = True Then
        '            If dr("Number") > 3 Then
        '                str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("SuccPt")) & "), " & _
        '                      "                     RAmtRate = " & CDbl(dr("SuccPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
        '                      "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' "
        '                cmd = New SqlCommand(str, cn)
        '                cmd.CommandTimeout = 0
        '                cmd.ExecuteNonQuery()
        '            Else
        '                str = "Update PF..PF Set RPF = 0,RAmtRate = 0,Computation1 = 'PF_Technician_OffDuty' " & _
        '                      "Where U_LabNo = '" & LabNo & "' and WhsCode = '" & WhsCodeT & "' and ItemCode = '" & ItemCode & "' "
        '                cmd = New SqlCommand(str, cn)
        '                cmd.CommandTimeout = 0
        '                cmd.ExecuteNonQuery()

        '                str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("FtPt")) & "), " & _
        '                      "                     RAmtRate = " & CDbl(dr("FtPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
        '                      "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' "
        '                cmd = New SqlCommand(str, cn)
        '                cmd.CommandTimeout = 0
        '                cmd.ExecuteNonQuery()
        '            End If
        '        End If

        '        LabNo = dr("u_labno")
        '        WhsCodeT = dr("whscode")
        '        ItemCode = dr("itemcode")

        '        WhsCode = dr("WhsCode")
        '        SAPCode1 = dr("SAPCode1")
        '        ProcDate = dr("ProcDate")
        '    End While
        'End If

        'str = "Drop table #Doctor"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        'str = "Drop table #tmp"
        'cmd = New SqlCommand(str, cn)
        'cmd.CommandTimeout = 0
        'cmd.ExecuteNonQuery()

        'dr.Close()
    End Sub

    Public Sub UpdateTechNicianRatesOffDutySpecialCase(ByVal cn As SqlConnection, cmd As SqlCommand)
        If cn.State = ConnectionState.Closed Then cn.Open()

        Dim str As String = "Select Distinct o.DSCode,SName,BID " & _
                            "Into #Doctor " & _
                            "From PF..PFDOCDET i With (Nolock) " & _
                            "INNER jOIN PF..PFDOCTORS o With (Nolock) on i.DocCode = o.DSCode " & _
                            "Where DType = 'Technician'"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        '//Special Case Technician --TechSpcl table
        str = "Select p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded)ProcDate,Sum(p.LineTotal)LineTotal,p.RRate,p.RAmtRate,Sum(p.RPF)RPF,Computation1, " & _
              "		ROW_NUMBER() over(PARTITION BY p.sapcode1,CONVERT(NVARCHAR(25), ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded), 111) ORDER BY ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded) ASC) AS Number " & _
              "Into #tmp " & _
              "From PF..PF p With (Nolock) " & _
              "Left Join ImagingDoctor i With (Nolock) on p.U_Labno = i.img_trxno and p.WhsCode = i.IMG_BRANCH and p.ItemCode = i.IMG_EXAM " & _
              "Left Join HPCOMMON..ImgTAT tat With (Nolock) on left(p.U_Labno,10) = tat.TrxNo and p.WhsCode = tat.WhsCode and p.ItemCode = tat.PendingExam " & _
              "Inner Join TechSpcl d on p.RCode = d.DCode and p.RName = d.DName and p.whscode = d.BID " & _
              "Inner Join pfRtRates r With (Nolock) on p.itemcode = r.itemcode and p.whscode = r.whscode and p.SAPCode1 = r.TechCode " & _
              "Where IsNull(GenID1,'') = '' and IsNull(Computation1,'') in ('','PF_Technician') " & _
              "Group By p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded),p.RRate,p.RAmtRate,Computation1 " & _
              "Having Sum(p.RPF) <> 0 " & _
              "Order By p.whscode,p.SAPCode1,p.docdate,p.u_labno "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select o.*,Case when o.DocDate >= i.EffDate Then i.FtPt Else i.PrevFtPt End FtPt, " & _
              "         Case when o.DocDate >= i.EffDate Then i.SuccPt Else i.PrevSuccPt End SuccPt " & _
              "From #tmp o With (Nolock) " & _
              "Inner Join pfrtdet i With (Nolock) on o.sapcode1 = i.TechCode and o.whscode = i.Whscode " & _
              "Order by o.whscode,SAPCode1,o.ProcDate,Number,o.u_labno"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader

        Dim WhsCode As String = ""
        Dim SAPCode1 As String = ""
        Dim ProcDate As String = ""
        Dim Number As String = ""

        Dim LabNo As String = ""
        Dim WhsCodeT As String = ""
        Dim ItemCode As String = ""

        If dr.HasRows Then
            While dr.Read
                Dim vChk As Boolean = False
                If WhsCode = "" Then WhsCode = dr("WhsCode")
                If SAPCode1 = "" Then SAPCode1 = dr("SAPCode1")
                If ProcDate = "" Then ProcDate = dr("ProcDate")
                If Number = "" Then Number = dr("Number")

                If WhsCode = dr("WhsCode") Then
                    If SAPCode1 = dr("SAPCode1") Then
                        If ProcDate = dr("ProcDate") Then
                            vChk = True
                        Else
                            vChk = False
                        End If
                    Else
                        vChk = False
                    End If
                Else
                    vChk = False
                End If

                If vChk = False Then
                    LabNo = ""
                    WhsCodeT = ""
                    ItemCode = ""
                    vChk = True
                End If

                If vChk = True Then
                    If dr("Number") > 3 Then
                        str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("SuccPt")) & "), " & _
                              "                     RAmtRate = " & CDbl(dr("SuccPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()
                    Else
                        str = "Update PF..PF Set RPF = 0,RAmtRate = 0,Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & LabNo & "' and WhsCode = '" & WhsCodeT & "' and ItemCode = '" & ItemCode & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()

                        str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("FtPt")) & "), " & _
                              "                     RAmtRate = " & CDbl(dr("FtPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()
                    End If
                End If

                LabNo = dr("u_labno")
                WhsCodeT = dr("whscode")
                ItemCode = dr("itemcode")

                WhsCode = dr("WhsCode")
                SAPCode1 = dr("SAPCode1")
                ProcDate = dr("ProcDate")
            End While
        End If
        dr.Close()
    End Sub

    Public Sub UpdateTechNicianRates_Oncall(ByVal cn As SqlConnection, cmd As SqlCommand)
        If cn.State = ConnectionState.Closed Then cn.Open()

        Try
            cmd = New SqlCommand("Drop Table #Doctor", cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        Try
            cmd = New SqlCommand("Drop Table #tmp", cn)
            cmd.CommandTimeout = 0
            cmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try

        Dim str As String = "Select Distinct o.DSCode,SName,BID,i.DCode,i.DName " & _
                            "Into #Doctor " & _
                            "From PF..PFDOCDET i With (Nolock) " & _
                            "INNER Join PF..PFDOCTORS o With (Nolock) on i.DocCode = o.DSCode " & _
                            "Where i.DType = 'Technician' AND ISNULL(i.TechOnCall,0) = 1 "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded)ProcDate,Sum(p.LineTotal)LineTotal,p.RRate,p.RAmtRate,Sum(p.RPF)RPF,Computation1, " & _
              "		ROW_NUMBER() over(PARTITION BY p.sapcode1,CONVERT(NVARCHAR(25), ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded), 111) ORDER BY ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded) ASC) AS Number " & _
              "Into #tmp " & _
              "From PF..PF p With (Nolock) " & _
              "Left Join ImagingDoctor i With (Nolock) on p.U_Labno = i.img_trxno and p.WhsCode = i.IMG_BRANCH and p.ItemCode = i.IMG_EXAM " & _
              "Left Join HPCOMMON..ImgTAT tat With (Nolock) on left(p.U_Labno,10) = tat.TrxNo and p.WhsCode = tat.WhsCode and p.ItemCode = tat.PendingExam " & _
              "Inner Join pfRtRates r With (Nolock) on p.itemcode = r.itemcode and p.whscode = r.whscode and p.SAPCode1 = r.TechCode " & _
              "Inner Join #Doctor t ON p.RCode = t.DCode AND p.RName = t.DName AND t.SName = p.SAPCode1 AND t.BID = p.whscode " & _
              "Where IsNull(GenID1,'') = '' and IsNull(Computation1,'') in ('','PF_Technician') " & _
              "Group By p.docdate,p.u_labno,p.whscode,p.itemcode,p.sapcode1,ISNULL(cast(i.IMG_ENCODED_DATE as date),tat.DateEncoded),p.RRate,p.RAmtRate,Computation1 " & _
              "Having Sum(p.RPF) <> 0 " & _
              "Order By p.whscode,p.SAPCode1,p.docdate,p.u_labno "
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        str = "Select o.*,Case when o.DocDate >= i.EffDate Then i.FtPt Else i.PrevFtPt End FtPt, " & _
              "         Case when o.DocDate >= i.EffDate Then i.SuccPt Else i.PrevSuccPt End SuccPt " & _
              "From #tmp o With (Nolock) " & _
              "Inner Join pfrtdet i With (Nolock) on o.sapcode1 = i.TechCode and o.whscode = i.Whscode " & _
              "Order by o.whscode,SAPCode1,o.ProcDate,Number,o.u_labno"
        cmd = New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader

        Dim WhsCode As String = ""
        Dim SAPCode1 As String = ""
        Dim ProcDate As String = ""
        Dim Number As String = ""

        Dim LabNo As String = ""
        Dim WhsCodeT As String = ""
        Dim ItemCode As String = ""

        If dr.HasRows Then
            While dr.Read
                Dim vChk As Boolean = False
                If WhsCode = "" Then WhsCode = dr("WhsCode")
                If SAPCode1 = "" Then SAPCode1 = dr("SAPCode1")
                If ProcDate = "" Then ProcDate = dr("ProcDate")
                If Number = "" Then Number = dr("Number")

                If WhsCode = dr("WhsCode") Then
                    If SAPCode1 = dr("SAPCode1") Then
                        If ProcDate = dr("ProcDate") Then
                            vChk = True
                        Else
                            vChk = False
                        End If
                    Else
                        vChk = False
                    End If
                Else
                    vChk = False
                End If

                If vChk = False Then
                    LabNo = ""
                    WhsCodeT = ""
                    ItemCode = ""
                    vChk = True
                End If

                If vChk = True Then
                    If dr("Number") > 3 Then
                        str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("SuccPt")) & "), " & _
                              "                     RAmtRate = " & CDbl(dr("SuccPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()
                    Else
                        str = "Update PF..PF Set RPF = 0,RAmtRate = 0,Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & LabNo & "' and WhsCode = '" & WhsCodeT & "' and ItemCode = '" & ItemCode & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()

                        str = "Update PF..PF Set RPF = (Case When ObjType = 13 Then 1 Else -1 End * " & CDbl(dr("FtPt")) & "), " & _
                              "                     RAmtRate = " & CDbl(dr("FtPt")) & ",Computation1 = 'PF_Technician_OffDuty' " & _
                              "Where U_LabNo = '" & dr("U_LabNo") & "' and WhsCode = '" & dr("WhsCode") & "' and ItemCode = '" & dr("ItemCode") & "' " & _
                              "     and ISNULL(GENID1,'') = '' "
                        cmd = New SqlCommand(str, cn)
                        cmd.CommandTimeout = 0
                        cmd.ExecuteNonQuery()
                    End If
                End If

                LabNo = dr("u_labno")
                WhsCodeT = dr("whscode")
                ItemCode = dr("itemcode")

                WhsCode = dr("WhsCode")
                SAPCode1 = dr("SAPCode1")
                ProcDate = dr("ProcDate")
            End While
        End If
        dr.Close()
    End Sub

    Private Sub GetLastUpd()
        Dim cn As New SqlConnection()
        cn = New SqlConnection("Data Source=" & SAPSrvr & ";Initial Catalog=" & DbCommon & ";Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                                 "Application Name = " & My.Application.Info.AssemblyName)
        If cn.State = ConnectionState.Closed Then cn.Open()

        Dim str As String = "SELECT PFMW FROM HPCOMMON..SAPOPT"
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim vTime As String = cmd.ExecuteScalar
        lblLastUpdTime.Text = "Last Update : " & vTime

        cmd.Dispose()
        cn.Dispose()
    End Sub

    Private Sub tmrProc_Tick(sender As Object, e As EventArgs) Handles tmrProc.Tick
        If ctr = 1800 Then
            tmrProc.Stop()
            btnProcess.PerformClick()
            ctr = 0
            tmrProc.Start()
        Else
            lblTime.Text = "Next update will be in " & 1800 - ctr & " second(s)."
            ctr += 1
        End If
    End Sub

    Private Sub frmTest_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.Modifiers = Keys.Control AndAlso e.KeyCode = Keys.Q Then
            If chkOverride.Enabled = False Then
                chkOverride.Enabled = True
            Else
                chkOverride.Enabled = False
                chkOverride.Checked = False
            End If
        End If
    End Sub

    Private Sub frmLISResult_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        GETSRVR()
        lblVersion.Text = "Version " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.MinorRevision
        lblVersion.BackColor = Color.Teal
        lblVersion.ForeColor = Color.Yellow
        lblTime.BackColor = Color.Teal
        lblTime.ForeColor = Color.Yellow
        lblDeveloper.BackColor = Color.Teal
        lblDeveloper.ForeColor = Color.Yellow
        tmrProc.Start()
        GetLastUpd()
    End Sub

    Private Sub TruncateSchedRmdr()
        Dim DTNow As DateTime = Format(Now(), "MM/dd/yyyy 21:00:00")
        Dim DTNow2 As DateTime = Format(DateAdd(DateInterval.Day, 1, Now()), "MM/dd/yyyy 04:00:00")
        If Now() >= DTNow And Now <= DTNow2 Then
            lblTime.Text = "Truncating Schedule Reminder, Please wait ..."
            Try
                Dim cnPF As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb;")
                If cnPF.State = ConnectionState.Closed Then cnPF.Open()
                Dim str As String = "Delete SchedRmdr WHERE CAST([TimeOut] AS TIME) < CAST(GETDATE() AS TIME)"
                Dim cmd As New SqlCommand(str, cnPF)
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub ImportDRC()
        Dim DTNow As DateTime = Format(Now(), "MM/dd/yyyy 20:00:00")
        Dim DTNow2 As DateTime = Format(DateAdd(DateInterval.Day, 1, Now()), "MM/dd/yyyy 05:00:00")
        If Now() >= DTNow And Now <= DTNow2 Then
            lblTime.Text = "Truncating Schedule Reminder, Please wait ..."
            Try
                ImportDRClinic()
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        tmrProc.Enabled = False
        tmrProc.Stop()
        Me.Cursor = Cursors.WaitCursor
        btnProcess.Enabled = False
        Try
            lblTime.Text = "Processing delete schedule reminder..."
            TruncateSchedRmdr()
        Catch ex As Exception : End Try
        EMS_Doctor_to_PF()
        lblTime.Text = "Calculating process please wait ..."
        ImportDRC()
        lblTime.Text = "Processing auto approve schedule..."
        AutoAppSched()
        lblTime.Text = "Processing auto close reliever transaction..."
        AutoCloseRlvrTrans()
        lblTime.Text = "Processing send out transaction..."
        'Process PF
        ProcessImport()
        Try
            ProcessImport_SendOut()
        Catch ex As Exception : End Try

        'IntellicareDeduction()
        'Try
        '    DoctorsClinicCheckAccountStatus()
        'Catch ex As Exception
        'End Try
    End Sub

    Private Sub DoctorsClinicCheckAccountStatus()
        Dim cn As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=DRCLINICS;Integrated Security=False;UID=sapdb;PWD=sapdb;")
        If cn.State = ConnectionState.Closed Then cn.Open()
        Dim str As String = "SELECT Srce,Code FROM DRClinics..sapset WITH (NOLOCK) WHERE Stat = 'O' ORDER BY Code"
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader
        If dr.HasRows Then
            While dr.Read
                Dim cnBr As New SqlConnection("Data Source=" & dr("Srce") & ";Initial Catalog=DRCLINICS;Integrated Security=False;UID=sapdb;PWD=sapdb")
                If cnBr.State = ConnectionState.Closed Then cnBr.Open()

                str = "SELECT TOP 1 1 FROM dbo.User_Master " & _
                      "WHERE ValidityDate > CAST(GETDATE() AS DATE) AND stat = 0 AND AccessLevel = 'Doctor' "
                cmd = New SqlCommand(str, cnBr)
                cmd.CommandTimeout = 0
                str = cmd.ExecuteScalar

                If Not str Is Nothing Then
                    str = "UPDATE User_Master SET stat = 1 " & _
                          "FROM (	SELECT * " & _
                          "         FROM dbo.User_Master With (Nolock) " & _
                          "		    WHERE ValidityDate > CAST(GETDATE() AS DATE) AND stat = 0 AND AccessLevel = 'Doctor' ) a " & _
                          "WHERE User_Master.UserID = a.UserID AND User_Master.PhysicianID = a.PhysicianID AND User_Master.AccessLevel = a.AccessLevel"
                    cmd = New SqlCommand(str, cnBr)
                    cmd.CommandTimeout = 0
                    cmd.ExecuteNonQuery()
                End If

                cnBr.Dispose()
            End While
        End If
        dr.Close()
    End Sub

    Private Sub IntellicareDeduction()
        Dim cnPyRl As SqlConnection = GetConn()
        If cnPyRl.State = ConnectionState.Closed Then cnPyRl.Open()

        Dim cnPF As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb;")
        If cnPF.State = ConnectionState.Closed Then cnPF.Open()

        Dim str As String = "SELECT ISNULL(MAX(TransID),0)TransID FROM PRDeduction WITH (NOLOCK)"
        Dim cmd As New SqlCommand(str, cnPF)
        cmd.CommandTimeout = 0
        Dim vTransID As Integer = cmd.ExecuteScalar

        str = "SELECT TransId,EmpCode,Code,PdType,DeducTotal,NoOfDeduc,Amortization,TotalPayment,Balance,Remarks,DeducType,Status,DocDate " & _
              "FROM PRDeduction WITH (NOLOCK) " & _
              "WHERE Code = 'D28' AND TransId > " & vTransID
        Using dt2 As New DataTable("PRDeduction")
            Using da2 As New SqlDataAdapter(str, cnPyRl)
                da2.Fill(dt2)
            End Using
            Using bulkCopy As New SqlBulkCopy(cnPF)
                bulkCopy.DestinationTableName = "PRDeduction"
                Try
                    'Write from the source to the destination.
                    bulkCopy.BulkCopyTimeout = 0
                    bulkCopy.WriteToServer(dt2)
                    bulkCopy.Close()
                Catch ex As Exception
                Finally
                End Try
            End Using
        End Using
    End Sub

    Public Function GetConn() As SqlConnection
        Return sqlconn.HPPYRL_Conn
    End Function

    Private Sub AutoCloseRlvrTrans()
        lblTime.Text = "Processing auto close reliever transaction..."
        Dim cn As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                                    "Application Name = " & My.Application.Info.AssemblyName)
        If cn.State = ConnectionState.Closed Then cn.Open()

        Dim str As String = "Exec AppPCFImaging"
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        cmd.ExecuteNonQuery()

        cmd.Dispose()
        cn.Dispose()
    End Sub

    Private Sub EMS_Doctor_to_PF()
        Dim DTNow As DateTime = Format(Now(), "MM/dd/yyyy 20:00:00")
        Dim DTNow2 As DateTime = Format(DateAdd(DateInterval.Day, 1, Now()), "MM/dd/yyyy 03:00:00")
        If Now() >= DTNow And Now <= DTNow2 Then
            lblTime.Text = "Processing EMS doctor's to PF..."
            Try
                Dim cn As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb;MultipleActiveResultSets=true;" &
                                    "Application Name = " & My.Application.Info.AssemblyName)
                If cn.State = ConnectionState.Closed Then cn.Open()

                Dim str As String = "Exec Sync_EMS_Doctor"
                Dim cmd As New SqlCommand(str, cn)
                cmd.CommandTimeout = 0
                cmd.ExecuteNonQuery()
                cmd.Dispose()
                cn.Dispose()
            Catch ex As Exception
            End Try
        End If
    End Sub

    Private Sub BackgroundWorker1_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        PBImportInv.Visible = False
        BackgroundWorker1.Dispose()
        Me.Cursor = Cursors.Default
        tmrProc.Enabled = True
        tmrProc.Start()
        lblTime.Text = "Next update will be in 300 second(s)."
        btnProcess.Enabled = True
    End Sub

    Private Sub ImportDRClinic()
        tmrProc.Enabled = False
        tmrProc.Stop()
        Me.Cursor = Cursors.WaitCursor
        lblTime.Text = "Importing doctor's clinic transaction, Please wait ..."

        Dim cn As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb")
        If cn.State = ConnectionState.Closed Then cn.Open()

        Dim str As String = "SELECT * FROM DRCLINICS..SAPSET WITH (NOLOCK) WHERE Stat = 'O' ORDER BY Code"
        Dim cmd As New SqlCommand(str, cn)
        cmd.CommandTimeout = 0
        Dim dr As SqlDataReader = cmd.ExecuteReader

        If dr.HasRows Then
            While dr.Read
                Try
                    Dim cnPF As New SqlConnection("Data Source=172.30.0.17;Initial Catalog=PF;Integrated Security=False;UID=sapdb;PWD=sapdb")
                    If cnPF.State = ConnectionState.Closed Then cnPF.Open()
                    Dim cmdDRCSAP As New SqlCommand

                    Dim cnBrnc As New SqlConnection("Data Source=" & dr("Srce") & ";Initial Catalog=DRCLINICS;Integrated Security=False;UID=sapdb;PWD=sapdb;application name=" & My.Application.Info.AssemblyName)
                    If cnBrnc.State = ConnectionState.Closed Then cnBrnc.Open()

                    str = "Select case when (Qty='' or Qty = 0)  then 1 else Qty end Qty1,*, " &
                          "     isnull(( select top 1  eNCODEDBY from R_HDR WHERE LABNO =T.LABNO) ,0) enc, " &
                          "     isnull(( select top 1  rphysicianid from R_HDR WHERE LABNO =T.LABNO) ,0) Doc,isnull(ApprovNo,'') ApprovNo " &
                          "FROM temptable t " &
                          "Where CAST(trxdate AS DATE)>= '1/1/2019' AND CAST(trxdate AS DATE) BETWEEN DATEADD(M,-6, CAST(GETDATE() AS DATE)) AND CAST(GETDATE() AS DATE) AND ttype = 'CASH' " &
                          "     and ttype = 'CASH' and t.labno not in (select labno from CancelTrx)  " &
                          "     order by labno"
                    cmd = New SqlCommand(str, cnBrnc)
                    cmd.CommandTimeout = 0
                    Dim dr1 As SqlDataReader = cmd.ExecuteReader

                    If dr1.HasRows Then
                        While dr1.Read
                            str = ""

                            str = " if not exists(select top 1 1 from pf..oinv3_Cash where Labno = '" & dr1("Labno") & "' and whscode='" & dr("Code") & "') " _
                                & " insert into pf..oinv3_Cash ( Labno,DocDate,CardCode,CardName,Patient,DCode,ItemCode,ItemName,PackageNo,LineTotal, " _
                                & "                       Quantity,DiscPrcnt,CollAmt,AdjAmt,SOANo,whscode,ENCODER,note,DateCreated,ApprovNo) values " _
                                & " ('" & dr1("Labno") & "', " _
                                & " '" & Format(dr1("trxDate"), "MM/dd/yyyy") & "', " _
                                & " '" & dr1("Source") & "', " _
                                & " '" & dr1("SourceCode") & "', " _
                                & " '" & dr1("patname") & "', " _
                                & " '" & Trim(dr1("Doc")) & "', " _
                                & " '" & dr1("ItemCode") & "', " _
                                & " '" & dr1("ItemName") & "', " _
                                & " '" & dr1("spricelistcode") & "', " _
                                & " '" & CDbl(dr1("unitprice")) & "', " _
                                & " '" & dr1("Qty1") & "','0','0','0','','" & Trim(dr("Code")) & "','" & dr1("ENC") & "','" & dr1("note") & "',GetDate(),'" & dr1("ApprovNo") & "') "
                            cmdDRCSAP = New SqlCommand(str, cnPF)
                            cmdDRCSAP.CommandTimeout = 0
                            cmdDRCSAP.ExecuteNonQuery()

                        End While
                    End If
                    cnBrnc.Close()
                    cmd.Dispose()
                    cmdDRCSAP.Dispose()
                    cnPF.Close()
                Catch ex As Exception
                End Try
            End While
        End If

        Me.Cursor = Cursors.Arrow
    End Sub
End Class