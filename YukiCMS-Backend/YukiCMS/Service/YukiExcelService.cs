using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
namespace YukiCMS.Service
{
    public class YukiExcelService
    {
        private readonly YukiUserService _userService;
        private readonly YukiCommitteeService _committeeService;
        private readonly YukiSeatService _seatService;
        public YukiExcelService(
                YukiUserService userService,
                YukiCommitteeService committeeService,
                YukiSeatService seatService
            )
        {
            _userService = userService;
            _committeeService = committeeService;
            _seatService = seatService;
        }
        public byte[] exportToExcel()
        {
            MemoryStream stream = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(stream))
            {
                ExcelWorksheet usersSheet = package.Workbook.Worksheets.Add("用户");
                ExcelWorksheet committeesSheet = package.Workbook.Worksheets.Add("会场");
                //committeesSheet.Cells[1, 1].Value = usersSheet.Cells[2, 1, uss, 1].FullAddressAbsolute;
                ExcelWorksheet seatSheet = package.Workbook.Worksheets.Add("席位");
                ExcelWorksheet committeeApplicationSheet = package.Workbook.Worksheets.Add("会场报名");
                var uss = createUsersSheet(usersSheet);
                var css = createCommitteesSheet(committeesSheet);
                var sss = createSeatsSheet(
                    seatSheet,
                    usersSheet.Cells[2, 1, uss, 23].FullAddressAbsolute,
                    committeesSheet.Cells[2, 1, css, 7].FullAddressAbsolute
                );
                var cass = createCommitteeApplicationSheet(
                    committeeApplicationSheet,
                    usersSheet.Cells[2, 1, uss, 23].FullAddressAbsolute,
                    committeesSheet.Cells[2, 1, css, 7].FullAddressAbsolute,
                    seatSheet.Cells[2, 2, sss, 2].FullAddressAbsolute,
                    seatSheet.Cells[2, 3, sss, 3].FullAddressAbsolute,
                    seatSheet.Cells[2, 6, sss, 6].FullAddressAbsolute
                );
                fillUserApplicationStatus(
                    usersSheet,
                    uss,
                    committeeApplicationSheet.Cells[2, 1, cass, 1].FullAddressAbsolute
                );
                package.Save();
            }
            return stream.ToArray();
        }
        public int createUsersSheet(ExcelWorksheet sheet)
        {
            var headers = new string[]
            {
                "UID","邮箱","姓名","性别","报名会场","代表","会场","退会","支付","住宿","提前住宿天数","延长住宿天数","期望室友",
                "身份证件类型","证件号","出生日期","国家","省级行政区","城市","学校","团体报名",
                "电话号码","QQ号","微信号","会议经历"
            };
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }
            var usersList = _userService.getList(u => true);
            for (int i = 0; i < usersList.Count; i++)
            {
                sheet.Cells[i + 2, 1].Value = usersList[i].uid;
                sheet.Cells[i + 2, 2].Value = usersList[i].email;
                sheet.Cells[i + 2, 3].Value = usersList[i].info.name;
                sheet.Cells[i + 2, 4].Value = usersList[i].info.sex;

                sheet.Cells[i + 2, 8].Value = usersList[i].isWithdrawaled ? "是" : "否";
                sheet.Cells[i + 2, 9].Value = usersList[i].isRegPaid ? "是" : "否";
                sheet.Cells[i + 2, 10].Value = usersList[i].accommodation.isGA ? "是" : "否";
                sheet.Cells[i + 2, 11].Value = usersList[i].accommodation.aheadDays;
                sheet.Cells[i + 2, 12].Value = usersList[i].accommodation.extendDays;
                sheet.Cells[i + 2, 13].Value = usersList[i].accommodation.appliedRoommateName;
                sheet.Cells[i + 2, 14].Value = usersList[i].info.residentIdType == Models.YukiUserResidentIdType.CNResidentID ? "居民身份证" : "其他";
                sheet.Cells[i + 2, 15].Value = usersList[i].info.residentId;
                sheet.Cells[i + 2, 16].Value = usersList[i].info.birthday;
                sheet.Cells[i + 2, 16].Style.Numberformat.Format = "YYYY-MM-DD";
                sheet.Cells[i + 2, 17].Value = usersList[i].info.state;
                sheet.Cells[i + 2, 18].Value = usersList[i].info.province;
                sheet.Cells[i + 2, 19].Value = usersList[i].info.city;
                sheet.Cells[i + 2, 20].Value = usersList[i].info.school;
                sheet.Cells[i + 2, 21].Value = usersList[i].info.isinSchoolGroup ? "是" : "否";
                sheet.Cells[i + 2, 22].Value = usersList[i].info.phoneNumber;
                sheet.Cells[i + 2, 23].Value = usersList[i].info.qqNumber;
                sheet.Cells[i + 2, 24].Value = usersList[i].info.wechatNumber;
                sheet.Cells[i + 2, 25].Value = usersList[i].info.cv;

            }
            sheet.Calculate();
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Column(i + 1).AutoFit();
            }
            return usersList.Count + 2;
        }
        public int createCommitteesSheet(ExcelWorksheet sheet)
        {
            var headers = new string[]
            {
                "ID","会场名","会场类型","互斥会场","会场简介","会费","退款额","缴费中"

            };
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }
            var comsList = _committeeService.getList(c => true);
            for (int i = 0; i < comsList.Count; i++)
            {
                sheet.Cells[i + 2, 1].Value = comsList[i].cid;
                sheet.Cells[i + 2, 2].Value = comsList[i].name;
                switch (comsList[i].ctype)
                {
                    case Models.YukiCommitteeType.appliableAdmCommittee:
                        sheet.Cells[i + 2, 3].Value = "可申请的非互斥会场";
                        sheet.Cells[i + 2, 4].Value = "否";
                        break;
                    case Models.YukiCommitteeType.appliableNormalMutualCommittee:
                        sheet.Cells[i + 2, 3].Value = "可申请的互斥性会场";
                        sheet.Cells[i + 2, 4].Value = "是";
                        break;
                    case Models.YukiCommitteeType.unappliable:
                        sheet.Cells[i + 2, 3].Value = "不可申请的非互斥会场";
                        sheet.Cells[i + 2, 4].Value = "否";
                        break;
                    case Models.YukiCommitteeType.unappliableMutual:
                        sheet.Cells[i + 2, 3].Value = "不可申请的互斥性会场";
                        sheet.Cells[i + 2, 4].Value = "是";
                        break;
                }
                sheet.Cells[i + 2, 5].Value = comsList[i].cdesc;
                sheet.Cells[i + 2, 6].Value = comsList[i].paymentSettings.price;
                sheet.Cells[i + 2, 7].Value = comsList[i].paymentSettings.refund;
                sheet.Cells[i + 2, 8].Value = comsList[i].paymentSettings.paymentEnabled ? "是" : "否";
            }
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Column(i + 1).AutoFit();
            }
            return comsList.Count + 2;
        }
        public int createSeatsSheet(ExcelWorksheet sheet, string usfa, string csfa)
        {
            var headers = new string[]
            {
                "ID","席位名","会场ID","会场名","状态","用户ID","用户名"
            };
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }
            var seatsList = _seatService.getList(s => true);
            for(int i = 0; i < seatsList.Count; i++)
            {
                sheet.Cells[i + 2, 1].Value = seatsList[i].sid;
                sheet.Cells[i + 2, 2].Value = seatsList[i].name;
                sheet.Cells[i + 2, 3].Value = seatsList[i].cid;
                sheet.Cells[i + 2, 4].Formula = string.Format("=VLOOKUP({0},{1},2)", sheet.Cells[i + 2, 3].Address, csfa);
                if (seatsList[i].status == Models.YukiSeatStatus.assigned) 
                {
                    sheet.Cells[i + 2, 5].Value = "已分配";
                    sheet.Cells[i + 2, 6].Value = seatsList[i].uid;
                    sheet.Cells[i + 2, 7].Formula = string.Format("=VLOOKUP({0},{1},3)", sheet.Cells[i + 2, 6].Address, usfa);
                }
                else
                {
                    sheet.Cells[i + 2, 5].Value = "未分配";
                }

            }
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Column(i + 1).AutoFit();
            }
            sheet.Column(4).Width = 60;
            return seatsList.Count + 2;
        }
        public int createCommitteeApplicationSheet(ExcelWorksheet sheet, string usfa, string csfa,string ssb,string ssc,string ssf)
        {
            var headers = new string[]
            {
                "用户ID","用户名","会场ID","会场名","席位","支付","代表"
            };
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
            }
            var comsList = _committeeService.getList(c => true);
            var cnt = 0;
            foreach(var c in comsList)
            {
                foreach(var uid in c.members)
                {
                    cnt++;
                    sheet.Cells[cnt + 1, 1].Value = uid;
                    sheet.Cells[cnt + 1, 2].Formula = string.Format("=VLOOKUP({0},{1},3)", sheet.Cells[cnt + 1, 1].Address, usfa);
                    sheet.Cells[cnt + 1, 3].Value = c.cid;
                    sheet.Cells[cnt + 1, 4].Formula = string.Format("=VLOOKUP({0},{1},2)", sheet.Cells[cnt + 1, 3].Address, csfa);
                    sheet.Cells[cnt + 1, 5].Formula = string.Format(
                        "=IFERROR(LOOKUP(1,0/(({0}={1})*({2}={3})),{4}),\"未分配\")",
                        ssf, sheet.Cells[cnt + 1, 1].Address,
                        ssc, sheet.Cells[cnt + 1, 3].Address,
                        ssb
                    );
                    sheet.Cells[cnt + 1, 6].Formula = string.Format("=VLOOKUP({0},{1},7)", sheet.Cells[cnt + 1, 1].Address, usfa);
                    sheet.Cells[cnt + 1, 7].Formula = string.Format("=VLOOKUP({0},{1},4)", sheet.Cells[cnt + 1, 3].Address, csfa);
                }
            }
            for (int i = 0; i < headers.Count(); i++)
            {
                sheet.Column(i + 1).AutoFit();
            }
            return cnt+1;
        }
        public void fillUserApplicationStatus(ExcelWorksheet sheet,int uss,string casa)
        {
            for(int i=2;i< uss; i++)
            {
                sheet.Cells[i, 5].Formula = string.Format(
                    "=IF(IFERROR(LOOKUP(1,0/({0}={1}),A$1:A$500),0)=0,\"未报名\",\"已报名\")",
                    casa,sheet.Cells[i,1].Address
                );
                sheet.Cells[i, 6].Formula = string.Format(
                    "=IFERROR(LOOKUP(1,0/({0}={1}),会场报名!$G$2:$G$500),\"未报名\")",
                    casa, sheet.Cells[i, 1].Address
                );
                sheet.Cells[i, 7].Formula = string.Format(
                    "=IFERROR(LOOKUP(1,0/({0}={1}),会场报名!$D$2:$D$500),\"未报名\")",
                    casa, sheet.Cells[i, 1].Address
                );
            }
            sheet.Column(5).AutoFit();
        }
    }
}
