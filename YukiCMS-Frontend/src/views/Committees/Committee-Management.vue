<template>
    <div>
        <div class="text-h5 mx-6 my-6">会场管理 / {{com!==null?com.name:"会场不存在"}}</div>
        <v-divider class="mx-9"/>
        <div 
            class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" 
            v-if="enabledPages.enableCommitteeManagement!==true">
            您没有查看的权限
        </div>
        <template v-else>
            <v-container class="mx-lg-9 my-6">
                <v-row>
                    <v-col class="text-h6">
                        基本信息
                    </v-col>
                </v-row>
                <v-row>
                    <v-col md="4" cols="12" class="px-lg-5">
                        <v-text-field label="会场名" v-model="com.name" />
                    </v-col>
                    <v-col md="4" cols="12" class="px-lg-5">
                        <v-select :items="availableCType" item-text="val" item-value="id" v-model="com.ctype" label="会场类型" />
                    </v-col>
                    <v-col md="4" cols="12" class="px-lg-5">
                        <v-dialog v-model="dialog" width="500" >
                            <template v-slot:activator="{ on, attrs }">
                                <v-text-field label="报名截止日期" v-model="com.applyDDL"  v-bind="attrs" v-on="on" readonly/>
                            </template>
                            <v-date-picker  v-model="com.applyDDL"/>
                        </v-dialog>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <v-textarea label="会场简介" v-model="com.cdesc" outlined shaped/>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col class="d-flex">
                        <v-spacer/>
                        <v-btn color="primary" class="mx-12" @click.stop="submitInfo()" :disabled="permission.editInfo!==true">保存</v-btn>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col class="text-h6">
                        缴费信息
                    </v-col>
                </v-row>
                <v-row>
                    <v-col md="4" cols="12" class="px-5">
                        <v-text-field label="会场会费" v-model="com.paymentSettings.price" :disabled="paymentSettingsEnabled!==true"/>
                    </v-col>
                    <v-col md="4" cols="12" class="px-5">
                        <v-text-field label="会场退费金额" v-model="com.paymentSettings.refund" :disabled="paymentSettingsEnabled!==true"/>
                    </v-col>
                    <v-col md="4" cols="12" class="px-5 d-flex align-center">
                        <v-switch
                            :disabled="paymentSettingsEnabled!==true" :readonly="paymentSettingsEnabled===true"
                            v-model="com.paymentSettings.paymentEnabled"
                            label="开启缴费" @click="com.paymentSettings.paymentEnabled=!com.paymentSettings.paymentEnabled;togglePaymentDialog=true"
                            class="mx-2"/>
                        <span v-if="const_ctype[com.ctype].paymentEnable!==true" class="text-body-2 deep-orange--text accent-2">非互斥会场无法开启缴费功能</span>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col class="d-flex">
                        <v-spacer/>
                        <v-btn color="primary" class="mx-12" :disabled="paymentSettingsEnabled!==true" @click.stop="submitPaymentSettings()">保存</v-btn>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col class="text-h6">
                        成员管理
                    </v-col>
                </v-row>
                <v-row v-if="users!==null">
                    <v-col>
                        <v-data-table :headers="users_header" :items.sync="users" class="mx-lg-12" calculate-widths :loading="loading" loading-text="加载中……" sort-by="uid">
                            <template v-slot:item.actions="{ item }">
                                <v-btn text color="primary" :disabled="permission.transferCommitteeMembers!==true" @click.stop="openUser(item)">转到</v-btn>
                                <v-btn text color="deep-orange accent-2" :disabled="permission.transferCommitteeMembers!==true" @click.stop="openDeleteMemberDialog(item)">移除</v-btn>
                            </template>
                        </v-data-table>
                    </v-col>
                </v-row>
            </v-container>
            <v-dialog v-model="notedialog.active" width="200">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text>
                        <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                        <p v-else class="d-flex justify-center">操作失败！</p>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="deleteMemberDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要从会场
                            <span  class="green--text darken-1"> {{com.name}} </span>
                            中移除代表
                            <span  class="deep-orange--text accent-2"> {{activeUser.name}} </span>
                            吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="deleteMemberDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="permission.transferCommitteeMembers!==true" @click="submitDeleteMember();deleteMemberDialog=false;">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="togglePaymentDialog" width="400" persistent>
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要
                            <span  class="deep-orange--text accent-2">
                                {{com.paymentSettings.paymentEnabled===true?'开启':'关闭'}} 
                            </span>
                            会场
                            <span  class="green--text darken-1"> {{com.name}} </span>
                            的缴费流程吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="togglePaymentDialog=false;com.paymentSettings.paymentEnabled=!com.paymentSettings.paymentEnabled">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="permission.transferCommitteeMembers!==true" @click="submitTogglePayment();togglePaymentDialog=false;">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </template>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
import {const_ctype} from '@/utils/committee-type'
export default {
    name:"Committee-Management",
    props:['cid'],
    mixins:[mixin],
    data:()=>({
        deleteMemberDialog:null,
        togglePaymentDialog:null,
        loading:false,
        activeUser:{},
        notedialog:{
            active:null,
            success:true,
        },
        com: null,
        originalctype:null,
        const_ctype,
        users_header:[
            {text:'UID',value:'uid',align:'center'},
            {text:'姓名',value:'name',align:'center'},
            {text:'性别',value:'sex',align:'center'},
            {text:'邮箱',value:'email',align:'center'},
            {text:'席位',value:'sname',align:'center'},
            {text:'学校',value:'school',align:'center'},
            {text:'会费支付',value:'isRegPaid_str',align:'center'},
            {text:'',value:'actions',sortable:false,align:'center'}
        ],
        users:null,
        permission:{
            editInfo:false,
            editPaymentSettings:false,
            transferCommitteeMembers:false,
            createCommittee:false,
        },
    }),
    computed:{
        paymentSettingsEnabled:function(){
            return this.permission.editPaymentSettings===true && this.const_ctype[this.com.ctype].paymentEnable===true
        },
        availableCType:function(){
            if(this.originalctype===null)return [];
            else if(this.const_ctype[this.originalctype].Mutual===true){
                if(this.com.paymentSettings.paymentEnabled===true){
                    return [this.const_ctype[0],this.const_ctype[3]]
                }
                else return this.const_ctype;
            }
            else return [this.const_ctype[1],this.const_ctype[2]]
        },
    },
    watch:{
        com:function(ncom){
            if(ncom!==null){
                this.originalctype=this.com.ctype;
                this.getPermission();
                this.initComStr();
                this.getMembers();
            }
            else{
                this.originalctype=null;
            }
        },
    },
    methods:{
        getCommittee:function(){
            var vm=this;
            this.fetchGet('/api/committee/c'+this.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.com=json;
                    })
                }
            })
        },
        getPermission:function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/committee/'+this.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        initComStr:function(){
            this.com.applyDDL=this.com.applyDDL.split(' ')[0];
        },
        getMembers:function(){
            var vm=this;
            this.fetchGet('/api/committee/c'+this.cid+'/members')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.users=[]
                        for(var i=0;i<json.length;i++){
                            vm.getMemberDetails(json[i])
                        }
                    })
                }
            })
        },
        getMemberDetails:async function(usr){
            var vm=this;
            var resinfo=await vm.fetchGet('/api/user/info/u'+usr.uid)
            var resreg=await vm.fetchGet('/api/user/paymentStatus/'+usr.uid)
            var resseat=await vm.fetchGet('/api/seat/u'+usr.uid+'/c'+vm.cid)
            if(resinfo.status===200){
                var jsoninfo=await resinfo.json()
                usr.sex=jsoninfo.sex,
                usr.school=jsoninfo.school;
            }
            if(resreg.status===200){
                var jsonreg=await resreg.json()
                usr.isRegPaid=jsonreg.isRegPaid,
                usr.isRegPaid_str=jsonreg.isRegPaid===true?'是':'否'
            }
            if(resseat.status===200){
                var jsonseat=await resseat.json()
                usr.sid=jsonseat.sid,
                usr.sname=jsonseat.name
            }
            else if(resseat.status===204){
                usr.sid=0;
                usr.sname="尚未分配"
            }
            vm.users.push(usr)
        },
        openDeleteMemberDialog:function(event){
            if(this.permission.transferCommitteeMembers!==true)return;
            this.activeUser=event;
            this.deleteMemberDialog=true;
        },
        submitInfo:function(){
            var vm=this;
            vm.fetchPutJson('/api/committee/c'+vm.cid+'/info',{
                name:vm.com.name,
                ctype:vm.com.ctype,
                applyDDL:vm.com.applyDDL,
                cdesc:vm.com.cdesc,
            })
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
        submitPaymentSettings:function(){
            var vm=this;
            vm.fetchPutJson('/api/committee/settings/payments/'+vm.cid,vm.com.paymentSettings)
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
        submitDeleteMember:function(){
            if(this.permission.transferCommitteeMembers!==true)return;
            var vm=this;
            this.fetchDelete('/api/committee/c'+this.cid+'/members/'+this.activeUser.uid)
            .then(function(res){
                if(res.status===204){
                    vm.users.splice(vm.users.indexOf(vm.activeUser),1);
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
        },
        submitTogglePayment:function(){
            if(this.paymentSettingsEnabled!==true)return;
            var vm=this;
            if(this.com.paymentSettings.paymentEnabled===true){
                vm.fetchPostJson('/api/committee/settings/payments/'+vm.cid,{})
                .then(function(res){
                    if(res.status===204){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    }
                    else{
                        this.com.paymentSettings.paymentEnabled=false;
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            }
            else{
                vm.fetchDelete('/api/committee/settings/payments/'+vm.cid)
                .then(function(res){
                    if(res.status===204){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    }
                    else{
                        this.com.paymentSettings.paymentEnabled=true;
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            }
        },
        openUser:function(item){
            this.$router.push({name:'User-Management',params:{uid:item.uid}})
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getCommittee())
    },
    beforeRouteUpdate (to, from, next) {
        this.getCommittee();
        next()
    },
}
</script>