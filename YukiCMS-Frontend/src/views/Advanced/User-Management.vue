<template>
    <div>
        <div class="text-h5 mx-6 my-6">用户管理</div>
        <v-divider class="mx-9"/>
        <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" v-if="enabledPages.enableUserManagement!==true || view_usr===null" >
            您没有查看的权限
        </div>
        <div v-else>
            <v-container >
                <v-row> 
                    <v-col sm="4" cols="12">
                        <v-text-field label="姓名" v-model="view_usr.info.name"/>
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-select
                            :items="['男','女']" 
                            label="性别"
                            v-model="view_usr.info.sex"
                            disabled
                            v-if="permission.viewInfo===true"
                        /> 
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-text-field label="邮箱" v-model="view_usr.email" disabled/>
                    </v-col>
                </v-row>
                    <template v-if="permission.viewInfo===true">
                    <v-row>
                        <v-col md="4" cols="12">
                            <v-select 
                                :items="[{hint:'身份证',value:0},{hint:'其它',value:1}]" 
                                item-text="hint" item-value="value" 
                                label="证件类型" 
                                v-model="view_usr.info.residentIdType" 
                            /> 
                        </v-col>
                        <v-col sm="6" md="4" cols="12">
                            <v-text-field label="证件号" v-model="view_usr.info.residentId"/>
                        </v-col>
                        <v-col sm="4" cols="12">
                            <v-dialog width="500" >
                                <template v-slot:activator="{ on, attrs }">
                                    <v-text-field label="出生日期" v-model="view_usr.info.birthday"  v-bind="attrs" v-on="on" readonly/>
                                </template>
                                <v-date-picker v-model="view_usr.info.birthday"/>
                            </v-dialog>
                        </v-col>
                    </v-row>
                    <v-row>
                            <v-col sm="4" cols="12">
                            <v-text-field label="国家" v-model="view_usr.info.state" />
                            </v-col>
                            <v-col sm="4" cols="12">
                            <v-text-field label="省级行政区" v-model="view_usr.info.province"/>
                            </v-col>
                            <v-col sm="4" cols="12">
                            <v-text-field label="城市"  v-model="view_usr.info.city"/>
                            </v-col>
                    </v-row>
                    <v-row>
                        <v-col sm="8" cols="12">
                            <v-text-field label="学校" v-model="view_usr.info.school" />
                        </v-col>
                        <v-col sm="4" cols="12">
                            <v-switch label="团体报名" v-model="view_usr.info.isinSchoolGroup"/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col sm="4" cols="12">
                            <v-text-field label="电话号码" name="phoneNumber" id="phoneNumber" v-model="view_usr.info.phoneNumber" />
                        </v-col>
                        <v-col sm="4" cols="12">
                            <v-text-field label="QQ号" name="qqNumber" id="qqNumber" v-model="view_usr.info.qqNumber" />
                        </v-col>
                        <v-col sm="4" cols="12">
                            <v-text-field label="微信号" name="wechatNumber" id="wechatNumber" v-model="view_usr.info.wechatNumber" />
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <v-textarea label="会议经历" name="cv" id="cv" v-model="view_usr.info.cv" outlined shaped auto-grow/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col/>
                        <v-col cols="auto">
                            <v-spacer/>
                            <v-btn outlined @click.stop="submitInfo()" :disabled="permission.editInfo!==true || withdrawStatus.withdrawed===true">保存</v-btn>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <v-divider/>
                        </v-col>
                    </v-row>
                    <v-row> 
                        <v-col cols="auto"  align-self="center">
                            <v-switch label="是否住宿" v-model="view_usr.accommodation.isGA"/>
                        </v-col>
                    </v-row>
                    <v-row v-if="view_usr.accommodation.isGA===true">
                        <v-col md="4" cols="12">
                            <v-text-field label="期望的室友姓名" v-model="view_usr.accommodation.appliedRoommateName"/>
                        </v-col>
                        <v-col md="4" sm="6" cols="12">
                            <v-slider label="提前住宿天数" min="0" max="2" ticks :tick-labels="['不提前','一日','二日']" v-model="view_usr.accommodation.aheadDays"/>
                        </v-col>
                        <v-col md="4" sm="6" cols="12">
                            <v-slider label="延长住宿天数" min="0" max="2" ticks :tick-labels="['不延长','一日','二日']" v-model="view_usr.accommodation.extendDays" />
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col/>
                        <v-col cols="auto">
                            <v-btn outlined @click.stop="submitAcc()" :disabled="permission.editInfo!==true || withdrawStatus.withdrawed===true">保存</v-btn>
                        </v-col>
                    </v-row>
                </template>
                <v-row>
                    <v-col>
                        <v-divider/>
                    </v-col>
                </v-row>
                <v-row v-if="permission.transferCommitteeMember===true && withdrawStatus.withdrawed!==true">
                    <v-col/>
                    <v-col cols="auto">
                        <v-btn outlined @click.stop="openJoinComDialog()">加入会场</v-btn>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <v-data-table :headers="com_headers" :items="incom" @dblclick:row="openDeleteMemberDialog"/>
                    </v-col>
                </v-row>
                <template v-if="permission.getUserPermissions===true && withdrawStatus.withdrawed!==true">
                    <v-row>
                        <v-col>
                            <v-divider/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col/>
                        <v-col cols="auto">
                            <v-btn outlined @click.stop="openGrantPermissionDialog()" v-if="permission.grantRevokeCusPermissions===true">独立赋权</v-btn>
                        </v-col>
                        <v-col cols="auto">
                            <v-btn outlined @click.stop="openJoinGroupDialog()" v-if="permission.cusGroupsTransferMember===true">加入自由权限组</v-btn>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <v-data-table :headers="pr_headers" :items.sync="viewed_response" @dblclick:row="selectPermissionItem" calculate-widths/>
                        </v-col>
                    </v-row>
                </template>
                <template v-if="permission.withdrawMembers===true && withdrawStatus.canWithdraw===true">
                    <v-row>
                        <v-col>
                            <v-divider class="mx-5 my-12"/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <div class="my-12"/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col/>
                        <v-col cols="auto">
                            <v-btn outlined color="deep-orange accent-2" @click="openRejoinDialog()" v-if="withdrawStatus.withdrawed===true">恢复报名</v-btn>
                            <v-btn outlined color="deep-orange accent-2" @click="openWithdrawDialog()" v-else>退会</v-btn>

                        </v-col>
                        <v-col/>
                    </v-row>
                    <v-dialog v-model="withdrawDialog" width="400">
                        <v-card>
                            <v-card-title>提示</v-card-title>
                            <v-card-text>
                                <div class="d-flex justify-center">
                                    <p class="mt-6">您真的确定要令代表
                                        <span class="primary--text"> {{view_usr.name}} </span>
                                        <span class="deep-orange--text accent-2"> 进入退会流程 </span>
                                        吗？
                                    </p>
                                </div>
                                <div class="d-flex justify-center">
                                    <p class="deep-orange--text accent-2">
                                        撤销代表的退会状态将可能造成一些未知的错误。
                                    </p>
                                </div>
                            </v-card-text>
                            <v-card-actions>
                                <v-spacer />
                                <v-btn text @click="withdrawDialog=false;">取消</v-btn>
                                <v-btn text color="deep-orange accent-2" :disabled="withdrawEnabled!==true" @click="submitWithdraw();">确定</v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                    <v-dialog v-model="rejoinDialog" width="400">
                        <v-card>
                            <v-card-title>提示</v-card-title>
                            <v-card-text>
                                <div class="d-flex justify-center">
                                    <p class="mt-6">您真的确定要令代表
                                        <span class="primary--text"> {{view_usr.name}} </span>
                                        <span class="deep-orange--text accent-2"> 恢复报名 </span>
                                        吗？
                                    </p>
                                </div>
                                <div class="d-flex justify-center">
                                    <p class="deep-orange--text accent-2">
                                        撤销代表的退会状态将可能造成一些未知的错误。
                                    </p>
                                </div>
                            </v-card-text>
                            <v-card-actions>
                                <v-spacer />
                                <v-btn text @click="rejoinDialog=false;">取消</v-btn>
                                <v-btn text color="deep-orange accent-2" :disabled="withdrawEnabled!==true" @click="submitRejoin();">确定</v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                </template>
                <v-dialog v-model="revokePermissionDialog" v-if="activePr!==null && permission.cusGroupsTransferMember===true" width="400">
                    <v-card>
                        <v-card-title>提示</v-card-title>
                        <v-card-text style="text-align: center;">
                            <p class="mt-6">您确定要移除权限
                                <span  class="green--text darken-1"> {{activePr.ptype_str}} </span>吗？
                            </p>
                            <p class="mx-10">
                                (权限作用域： 
                                <span  class="primary--text">{{activePr.scope}}</span>
                                — 
                                <span  class="deep-orange--text accent-2">{{activePr.pName}}</span>
                                )
                            </p>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer />
                            <v-btn text @click="revokePermissionDialog=false;">取消</v-btn>
                            <v-btn text color="deep-orange accent-2" @click="submitRevokePermission();">确定</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
                <v-dialog v-model="displayGranterDialog" v-if="granterList!==null" width="400">
                    <v-card>
                        <v-card-title>提示</v-card-title>
                        <v-card-subtitle>该权限共有以下赋权者</v-card-subtitle>
                        <v-card-text>
                            <p class="d-flex justify-center green--text darken-1 my-0" v-for="(granter,idx) of granterList" :key="idx">
                                {{granter.granterName}}
                            </p>
                        </v-card-text>
                    </v-card>
                </v-dialog>
                <v-dialog v-model="grantPermissionDialog" width="400" v-if="permission.grantRevokeCusPermissions===true">
                    <v-card width="400">
                        <v-toolbar dense flat color="blue lighten-4">
                            <v-toolbar-title>赋权</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <div class="mx-3 mt-6">
                                <v-select :items="visiblePermissionList" item-text="name" item-value="value" label="权限" v-model="newPer.ptype" @change="adjustNewPerPObjectId()"/>
                                <v-select :items="committee_list" item-text="name" item-value="cid" label="作用域（会场）" v-model="newPer.pObjectId" v-if="newPer.ptype!==null && Permisson_List[newPer.ptype].committee===true" />
                                <v-select :items="filegroup_list" item-text="name" item-value="fgid" label="作用域（文件组）" v-model="newPer.pObjectId" v-if="newPer.ptype!==null && Permisson_List[newPer.ptype].filegroup===true"/>
                                <div v-if="newPer.ptype!==null && Permisson_List[newPer.ptype].prerequisities">
                                    <p>该权限的正常生效依赖于以下权限：</p>
                                    <p class="d-flex justify-center deep-orange--text accent-2" v-for="(type,idx) of Permisson_List[newPer.ptype].prerequisities" :key="idx">
                                        {{Permisson_List[type].name}}
                                    </p>
                                </div>
                            </div>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer />
                            <v-btn class="mx-2" color="blue lighten-4" :disabled="newPer.pObjectId===null" @click.stop="submitGrantPermission()">赋权</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
                <v-dialog v-model="joinGroupDialog" width="400" v-if="permission.cusGroupsTransferMember===true">
                    <v-card width="400">
                        <v-toolbar dense flat color="blue lighten-4">
                            <v-toolbar-title>加入自由权限组</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <div class="mx-3 mt-6">
                                <v-select :items="freegroup_list" item-text="name" item-value="pgid" label="权限组" v-model="newpgid"/>
                            </div>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer />
                            <v-btn class="mx-2" color="blue lighten-4" :disabled="newpgid===null" @click.stop="submitJoinGroup()">加入</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
                <v-dialog v-model="joinComDialog" width="400"  v-if="permission.transferCommitteeMember===true">
                    <v-card width="400">
                        <v-toolbar dense flat color="blue lighten-4">
                            <v-toolbar-title>加入会场</v-toolbar-title>
                            <v-spacer></v-spacer>
                        </v-toolbar>
                        <v-card-text>
                            <div class="mx-3 mt-6">
                                <v-select :items="availcom" item-text="name" item-value="cid" label="会场" v-model="newcid"/>
                            </div>
                        </v-card-text>
                        <v-card-actions>
                            <v-spacer />
                            <v-btn class="mx-2" color="blue lighten-4" :disabled="newcid===null" @click.stop="submitJoinCom()">加入</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </v-container>
        </div>
        <v-dialog v-model="notedialog.active" width="200">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                    <p v-else class="d-flex justify-center">操作失败！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-dialog v-model="deleteMemberDialog" width="400" v-if="activeCom!==null&& permission.transferCommitteeMember===true" >
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <div class="d-flex justify-center">
                        <p class="mt-6">您确定要从会场
                            <span  class="green--text darken-1"> {{activeCom.name}} </span>
                            中移除代表
                            <span  class="deep-orange--text accent-2"> {{view_usr.name}} </span>
                            吗？
                        </p>
                    </div>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn text @click="deleteMemberDialog=false;">取消</v-btn>
                    <v-btn text color="deep-orange accent-2" @click="submitDeleteMember();">确定</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>
<script>
import {const_ctype} from '@/utils/committee-type'
import {Permisssion_Type,Permisson_List} from "@/utils/permission"
import mixin from '@/utils/mixins'
import lodash from 'lodash'
export default {
    name:"User-Management",
    props:['uid'],
    mixins:[mixin],
    data:()=>({
        notedialog:{
            active:null,
            success:true,
        },
        newPer:{
            ptype:null,
            pObjectId:null,
        },
        newpgid:null,
        newcid:null,
        activePr:null,
        activeCom:null,
        grantPermissionDialog:null,
        revokePermissionDialog:null,
        displayGranterDialog:null,
        joinGroupDialog:null,
        joinComDialog:null,
        view_usr:null,
        deleteMemberDialog:null,
        com_headers:[
            {text:'序号',value:'cid'},
            {text:'会场名',value:'name'},
            {text:'类型',value:'ctype_str'},
            {text:'席位',value:'seat'},

        ],
        pr_headers:[
            {text:'权限',value:'ptype_str'},
            {text:'类型',value:'scope'},
            {text:'作用域',value:'pName'},
            {text:'赋权者个数',value:'pGranterCount'},
            {text:'独立赋权',value:'customizeGranted_str'}
        ],
        permission_response:[],
        viewed_response:[],
        granterList:[],
        committee_list:[],
        filegroup_list:[],
        freegroup_list:[],
        incom:[],
        availcom:[],
        permission:{
            viewInfo:false,
            editInfo:false,
            transferCommitteeMember:false,
            getUserPermissions:false,
            grantRevokeCusPermissions:false,
            cusGroupsTransferMember:false,
            withdrawMembers:false,
        },
        withdrawDialog:null,
        rejoinDialog:null,
        withdrawEnabled:false,
        withdrawStatus:{
            withdrawed:false,
            canWithdraw:false,
        },
        Permisssion_Type,
        Permisson_List,
        const_ctype,
    }),
    watch:{
        view_usr:function(){
            this.getPermission();
            this.getWithdrawPermission();
            this.getPermissionResponse();
            this.getCommitteeList();
            this.getFileGroupList();
            this.getFreeGroupList();
            this.getIncom();
            this.getAvailCom();
        },
        permission_response:function(){
            this.viewed_response=[];
            this.initPRPermissionList();

        }
    },
    computed:{
        visiblePermissionList:function(){
            return this.Permisson_List.slice(1);
        },
    },
    methods:{
        getUsr:async function(){
            var vm=this;
            var ubres=await this.fetchGet('/api/user/name/'+vm.uid)
            if(ubres.status!==200)return;
            var user=await ubres.json();
            var resinfo=await vm.fetchGet('/api/user/info/u'+vm.uid)
            var resreg=await vm.fetchGet('/api/user/paymentStatus/'+vm.uid)
            if(resinfo.status===200){
                var jsoninfo=await resinfo.json()
                jsoninfo.birthday=jsoninfo.birthday.split(" ")[0];
                user.info=jsoninfo;
            }
            if(resreg.status===200){
                var jsonreg=await resreg.json()
                user.accommodation=jsonreg.accommodation,
                user.isRegPaid=jsonreg.isRegPaid;
                user.isWithdrawaled=jsonreg.isWithdrawaled;
            }
            vm.view_usr=user;
        },
        getPermission:async function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/usermanagement')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        getWithdrawPermission:async function(){
            var vm=this;
            this.fetchGet('/api/permission/vertify/withdraw/'+this.uid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.withdrawStatus=json;
                    })
                }
            })
        },
        getCommitteeList:function(){
            var vm=this;
            this.fetchGet('/api/committee')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.committee_list=json;
                    })
                }
            })
        },
        getFileGroupList:function(){
            var vm=this;
            this.fetchGet('/api/filegroup/list')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.filegroup_list=json;
                    })
                }
            })
        },
        getFreeGroupList:function(){
            var vm=this;
            this.fetchGet('/api/permission/group/free')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.freegroup_list=json;
                    })
                }
            })
        },
        getIncom:function(){
            var vm=this;
            this.fetchGet('/api/user/committee/'+this.uid)
            .then(function(res){
                if(res.status===200){
                res.json().then(async function(json){
                        for(var i=0;i<json.length;i++){
                            json[i].ctype_str=vm.const_ctype[json[i].ctype].val;
                            var ress=await vm.fetchGet('/api/seat/u'+vm.uid+'/c'+json[i].cid)
                            if(ress.status===200){
                                var jsons=await ress.json();
                                json[i].seat=jsons.name;
                            }
                        }
                        vm.incom=json;
                    })
                }
            })
        },
        getAvailCom:function(){
            var vm=this;
            this.fetchGet('/api/committee/available/'+vm.uid)
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.availcom=json;
                    })
                }
            })
        },
        getPermissionResponse:function(){
            var vm=this;
            vm.selfpermissions=null;
            this.fetchGet('/api/permission/u'+ vm.uid)
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.permission_response=json.list;
                        vm.permission.grantRevokeCusPermissions=json.grantEnable;
                    })
                }
            })
        },
        initPRPermissionList:function(){
            for(var i=0;i<this.permission_response.length;i++){
                var pitems=this.permission_response[i];
                var pt=this.Permisson_List[pitems.type];
                for(var j=0;j<pitems.pObject.length;j++){
                    var poi=pitems.pObject[j];
                    var pitem={
                        ptype:pitems.type,
                        ptype_str:pt.name,
                        scope:null,
                        pObjectId:poi.pObjectId,
                        pName:poi.pObjectName,
                        pGranterCount:poi.pGranterCount,
                        customizeGranted:poi.customizeGranted,
                        customizeGranted_str:poi.customizeGranted===true?'是':'否'
                    }
                    if(pt.global===true)
                    {
                        pitem.scope='全局权限';
                        pitem.pName='全局';
                    }
                    else if (pt.committee===true)
                        pitem.scope='会场权限';
                    else if (pt.filegroup===true)
                        pitem.scope='文件组权限';
                    this.viewed_response.push(pitem);
                }
            }
        },
        submitInfo:function(){
            var vm=this;
            this.fetchPutJson('/api/user/info/u'+this.uid,this.view_usr.info)
                .then(function(res){
                    if(res.status===200){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    }
                    else{
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
        },
        submitAcc:function(){
            var vm=this;
            this.fetchPutJson('/api/user/acc/'+vm.uid,vm.view_usr.accommodation)
            .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
        },
        selectPermissionItem:async function(_,event){
            this.activePr=event.item;
            if(this.activePr.customizeGranted===true){
                this.revokePermissionDialog=true;
            }
            else{
                var res=await this.fetchGet("api/permission/u"+this.uid+"/"+this.activePr.ptype+"/"+this.activePr.pObjectId+"/granter")
                if(res.status!==200)this.granterList=[{granterName:"查询失败"}];
                else{
                    var json=await res.json();
                    this.granterList=json;
                }
                this.displayGranterDialog=true;
            }
        },

        submitRevokePermission:function(){
            var vm=this;
            vm.fetchDelete('/api/permission/u'+vm.uid+"/"+vm.activePr.ptype+'/'+vm.activePr.pObjectId)
                .then(function(res){
                if(res.status===204){
                    vm.viewed_response.splice(vm.viewed_response.indexOf(vm.activePr),1);
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.revokePermissionDialog=false;
        },
        openGrantPermissionDialog:function(){
            this.newPer.ptype=null;
            this.newPer.pObjectId=null;
            this.grantPermissionDialog=true;
        },
        adjustNewPerPObjectId:function(){
            if(this.newPer.ptype!==null && this.Permisson_List[this.newPer.ptype].global===true)
                this.newPer.pObjectId=0;
            else
                this.newPer.pObjectId=null;
        },
        submitGrantPermission:function(){
            var vm=this;
            vm.fetchPostJson('/api/permission/u'+vm.uid+"/"+vm.newPer.ptype+"/"+vm.newPer.pObjectId,{})
                .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getPermissionResponse();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.grantPermissionDialog=false;
        },
        openJoinGroupDialog:function(){
            this.newpgid=null;
            this.joinGroupDialog=true;
        },
        openJoinComDialog:function(){
            this.newcid=null;
            this.joinComDialog=true;
        },
        submitJoinGroup:function(){
            var vm=this;
            vm.fetchPostJson('/api/permission/group/'+vm.newpgid+'/u'+vm.uid,{})
                .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getPermissionResponse();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.joinGroupDialog=false;
        },
        submitJoinCom:function(){
            var vm=this;
            vm.fetchPostJson('/api/committee/c'+vm.newcid+'/members/'+vm.uid,{})
                .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getAvailCom();
                    vm.getIncom();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.joinComDialog=false;
        },
        openDeleteMemberDialog:function(_,event){
            if(this.permission.transferCommitteeMember===true){
                this.activeCom=event.item;
                this.deleteMemberDialog=true;
            }
        },
        submitDeleteMember:function(){
            var vm=this;
            this.fetchDelete('/api/committee/c'+this.activeCom.cid+'/members/'+this.view_usr.uid)
            .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getAvailCom();
                    vm.getIncom();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.deleteMemberDialog=false;
        },
        openWithdrawDialog:function(){
            this.withdrawEnabled=false;
            this.withdrawDialog=true;
            this.debounceEnableWithdrawSubmit();
        },
        enableWithdrawSubmit:function(){
            this.withdrawEnabled=true;
        },
        submitWithdraw:function(){
            var vm=this;
            this.fetchPostJson('/api/user/withdraw/'+this.uid,{})
                .then(function(res){
                    if(res.status===204){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                        vm.getUsr();
                    }
                    else{
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            this.withdrawDialog=false;
        },
        openRejoinDialog:function(){
            this.withdrawEnabled=false;
            this.rejoinDialog=true;
            this.debounceEnableWithdrawSubmit();
        },
        enableRejoinSubmit:function(){
            this.withdrawEnabled=true;
        },
        submitRejoin:function(){
            var vm=this;
            this.fetchDelete('/api/user/withdraw/'+this.uid)
                .then(function(res){
                    if(res.status===204){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                        vm.getUsr();
                    }
                    else{
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            this.rejoinDialog=false;
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getUsr())
    },
    beforeRouteUpdate (to, from, next) {
        this.getUsr();
        next()
    },
    created(){
        this.debounceEnableWithdrawSubmit=lodash.debounce(this.enableWithdrawSubmit, 5000)
    }
}
</script>