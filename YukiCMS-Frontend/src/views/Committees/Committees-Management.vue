<template>
    <div>
        <div class="text-h5 mx-6 mt-6">会场管理</div>
        <div v-if="enabledPages.enableCommitteeManagement!==true">
            <v-divider class="mx-9"/>
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" >您没有查看的权限</div>
        </div>
        <template v-else>
            <div class="d-flex">
                <v-spacer />
                <v-btn text class="grey--text darken-2 align-self-center mx-9" @click.stop="openCreateCommitteeDialog()" v-if="permission.createCommittee===true">创建会场</v-btn>
            </div>
            <v-divider class="mx-9"/>
            <template v-if="coms!==null">
                <v-data-table :headers="com_header" :items="coms" @click:row="expand" class="mx-12 my-5" calculate-widths :loading="loading" loading-text="加载中……" :expanded.sync="expanded" show-expand single-expand  item-key="cid">
                    <template v-slot:expanded-item="{ headers, item }">
                        <td :colspan="headers.length" @dblclick.stop="$router.push({name:'Committee-Management',params:{cid:item.cid}})">
                            <v-card flat class="my-1">
                                <v-card-text>
                                    <v-container fluid class="text-body-2">
                                        <v-row dense>
                                            <v-col md="4" cols="12">
                                                会场名称：
                                                <span class="blue--text darken-1">{{item.name}}</span>
                                            </v-col>
                                            <v-col md="4" cols="12" >
                                                会场类型：
                                                <span :class="const_ctype[item.ctype].class">{{const_ctype[item.ctype].val}}</span>
                                            </v-col>
                                            <v-col md="4" cols="12">
                                                自动推送中的学测数：{{item.autoPushedTasksCount}}
                                            </v-col>
                                        </v-row>
                                        <v-row align-content="center" dense>
                                            <v-col md="4" cols="12" align-self="center">
                                                当前会场会费：{{item.paymentSettings.price}}
                                            </v-col>
                                            <v-col md="4" cols="12" align-self="center">
                                                当前会场退费额度：{{item.paymentSettings.refund}}
                                            </v-col>
                                            <v-col lg="4" cols="12" align-self="center" class="d-flex justify-start flex-wrap align-center" v-if="permission.editPaymentSettings===true ">
                                                开启缴费：
                                                <v-switch
                                                    :disabled="item.ctype===1 || item.ctype===2" 
                                                    :readonly="item.ctype===0 || item.ctype===3" 
                                                    v-model="item.paymentSettings.paymentEnabled"
                                                    dense class="mx-2" @click="item.paymentSettings.paymentEnabled=!item.paymentSettings.paymentEnabled;active_com=item;togglePaymentDialog=true;"/>
                                                <span v-if="item.ctype===1 || item.ctype===2" class="deep-orange--text accent-2">非互斥会场无法开启缴费功能</span>
                                            </v-col>
                                        </v-row>
                                        <v-row dense>
                                            <v-col >
                                                <p>会场简介：</p>
                                                <pre class="text-body-2 mx-5" style="white-space: pre-wrap; word-wrap: break-word;">{{item.cdesc}}</pre>
                                            </v-col>
                                        </v-row>
                                        <v-row>
                                            <v-col cols="12" class="d-flex justify-center grey--text lighten-1 align-self-auto">
                                                点击以访问会场设置
                                            </v-col>
                                        </v-row>
                                    </v-container>
                                </v-card-text>
                            </v-card>
                        </td>
                    </template>
                    <template v-slot:item.data-table-expand />
                </v-data-table>
            </template>
            <v-dialog v-model="notedialog.active" width="200">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text>
                        <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                        <p v-else class="d-flex justify-center">操作失败！</p>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="createCommitteeDialog" width="400">
                <v-card width="400">
                    <v-toolbar flat dense color="blue lighten-4">
                        <v-toolbar-title>创建会场</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-form class="mx-5">
                            <v-text-field label="会场名" name="name" v-model="new_com.name"/>
                            <v-select :items="const_ctype" item-text="val" item-value="id" label="会场类型" v-model="new_com.ctype" />
                            <v-dialog width="500" >
                                <template v-slot:activator="{ on, attrs }">
                                    <v-text-field label="报名截止日期" v-model="new_com.applyDDL"  v-bind="attrs" v-on="on" readonly/>
                                </template>
                                <v-date-picker  v-model="new_com.applyDDL"/>
                            </v-dialog>
                            <v-textarea label="会场简介" v-model="new_com.cdesc" outlined shaped/>
                        </v-form>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn class="mx-2" color="blue lighten-4" :disabled="permission.createCommittee!==true" @click="submitCreate();createCommitteeDialog=false">创建</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="togglePaymentDialog" width="400" persistent>
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要
                            <span  class="deep-orange--text accent-2">
                                {{active_com.paymentSettings.paymentEnabled===true?'开启':'关闭'}} 
                            </span>
                            会场
                            <span  class="green--text darken-1"> {{active_com.name}} </span>
                            的缴费流程吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="togglePaymentDialog=false;active_com.paymentSettings.paymentEnabled=!active_com.paymentSettings.paymentEnabled">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="permission.editPaymentSettings!==true" @click="submitTogglePayment();togglePaymentDialog=false;">确定</v-btn>
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
    name:"Committees-Management",
    mixins:[mixin],
    data:()=>({
        expanded:[],
        loading:null,
        togglePaymentDialog:null,
        createCommitteeDialog:null,
        active_com:{
            paymentSettings:{},
        },
        new_com:{
            name:null,
            ctype:null,
            applyDDL:null,
            cdesc:null,
        },
        notedialog:{
            active:null,
            success:true,
        },
        com_header:[
            {text:'会场名',value:'name'},
            {text:'是否互斥',value:"isMutual_str"},
            {text:'是否可申请',value:'isAppliable_str'},
            {text:'报名截止日期',value:'applyDDL'},
            {text:'报名人数',value:'membersCount'},
            {text:'已缴费人数',value:'paidMembersCount'},
            {text:'是否正在缴费',value:'paymentSettings.paymentEnabled_str'},
        ],
        coms:null,
        permission:{
            editPaymentSettings:false,
            createCommittee:false,
        },
        const_ctype,
    }),
    watch:{
        coms:function(ncoms){
            if(ncoms!==null){
                this.getPermission()
                this.initComStr();
            }
        }
    },
    methods:{
        expand: function(event){
            this.expanded=((this.expanded!==null&&this.expanded.length>0&&this.expanded[0].cid===event.cid)?[]:[event])
        },
        submitTogglePayment:function(){
            if(this.permission.editPaymentSettings!==true)return;
            var vm=this;
            if(this.active_com.paymentSettings.paymentEnabled===true){
                vm.fetchPostJson('/api/committee/settings/payments/'+vm.active_com.cid,{})
                .then(function(res){
                    if(res.status===204){
                        vm.active_com.paymentSettings.paymentEnabled_str='是'
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    }
                    else{
                        this.active_com.paymentSettings.paymentEnabled=false;
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            }
            else{
                vm.fetchDelete('/api/committee/settings/payments/'+vm.active_com.cid)
                .then(function(res){
                    if(res.status===204){
                        vm.active_com.paymentSettings.paymentEnabled_str='否'
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    }
                    else{
                        this.active_com.paymentSettings.paymentEnabled=true;
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            }
        },
        getPermission:function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/committee')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        openCreateCommitteeDialog:function(){
            this.new_com={
                name:null,
                ctype:null,
                applyDDL:null,
                cdesc:null,
            }
            this.createCommitteeDialog=true;
        },
        submitCreate:function(){
            if(this.permission.createCommittee!==true)return;
            var vm=this;
            vm.fetchPostJson('/api/committee',vm.new_com)
            .then(function(res){
                if(res.status===201){
                    res.json().then(function(json){
                        var ct=vm.const_ctype[json.ctype];
                        json.isMutual_str=ct.Mutual?'是':'否',
                        json.isAppliable_str=ct.Appliable?'是':'否',
                        json.applyDDL=json.applyDDL.split(' ')[0];
                        json.membersCount=0;
                        json.paidMembersCount=0;
                        json.autoPushedTasksCount=0;
                        json.paymentSettings.paymentEnabled_str=json.paymentSettings.paymentEnabled===true?'是':'否';
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                        vm.coms.push(json)
                    })
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.notedialog.active=true;
        },
        getCommittees:function(){
            var vm=this;
            this.fetchGet('/api/committee/detailed')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.coms=json;
                    })
                }
            })
        },
        initComStr:function(){
            for(var i=0;i<this.coms.length;i++){
                var com=this.coms[i];
                var ct=this.const_ctype[com.ctype];
                com.isMutual_str=ct.Mutual?'是':'否',
                com.isAppliable_str=ct.Appliable?'是':'否',
                com.applyDDL=com.applyDDL.split(' ')[0];
                com.paymentSettings.paymentEnabled_str=com.paymentSettings.paymentEnabled===true?'是':'否';
            }
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getCommittees())
    },
    beforeRouteUpdate (to, from, next) {
        this.getCommittees();
        next()
    },
}
</script>