<template>
    <div>
        <div class="text-h5 mx-6 mt-6">账单管理</div>
        <template v-if="enabledPages.enablePaymentManagement!==true">
            <v-divider class="mx-9 mt-6"/>
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto"  >
                您没有查看的权限
            </div>
        </template>
        <template v-else>
            <div class="d-flex align-center mx-9">
                <v-tabs v-model="tab" right center-active show-arrows>
                    <v-tab>付款账单</v-tab>
                    <v-tab>退款账单</v-tab>
                    <v-btn text class="grey--text darken-2 align-self-center" @click.stop="openSearchSCDialog()">查找特征码</v-btn>
                </v-tabs>
            </div>

            <v-container>
                <v-row>
                    <v-col>
                        <v-tabs-items v-model="tab" >
                            <v-tab-item>
                                <div class="d-flex">
                                    <v-spacer/>
                                    <v-text-field v-model="reg_search" append-icon="mdi-magnify" label="搜索" single-line hide-details/>
                                </div>
                                <v-data-table :headers="header" :items="reg_bills" item-key="billid" @click:row="showBill" :search="reg_search"> 
                                </v-data-table>
                            </v-tab-item>
                            <v-tab-item>
                                <div class="d-flex">
                                    <v-spacer/>
                                    <v-text-field v-model="wd_search" append-icon="mdi-magnify" label="搜索" single-line hide-details/>
                                </div>
                                <v-data-table :headers="header" :items="wd_bills" item-key="billid" @click:row="showBill" :search="wd_search"> 
                                </v-data-table>
                            </v-tab-item>
                        </v-tabs-items>
                    </v-col>
                </v-row>
            </v-container>
            <v-dialog v-model="searchSCDialog" width="400">
                <v-card width="400">
                    <v-toolbar dense flat color="blue lighten-4">
                        <v-toolbar-title>查找特征码</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-text-field label="特征码" v-model="sc" @keypress.enter="searchSC()"/>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn class="mx-2" color="blue lighten-4" :disabled="sc===null || sc.length!==6" @click.stop="searchSC()">搜索</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="activeBillDialog" v-if="activebill!==null" width="600">
                <v-card width="600">
                    <v-toolbar dense flat color="blue lighten-4">
                        <v-toolbar-title>
                            查看账单 
                            <template v-if="permission.modifyPayment===true && const_bill_type[activebill.type].isRefund!==true && const_bill_status[activebill.status].unpaid===true">
                                / 确认支付
                            </template>
                        </v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-container>
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="账单号" v-model="activebill.billid" readonly/>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="代表姓名" v-model="activebill.uname" readonly/>
                                </v-col>
                            </v-row>
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="类型" v-model="activebill.type_hint" readonly/>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="支付金额" hint="若该账单实际需支付金额与此不符 可进行修改" persistent-hint v-model="activebill.amount"/>
                                </v-col>
                            </v-row>
                            <v-row>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="状态" v-model="activebill.status_hint" readonly/>
                                </v-col>
                                <v-col cols="12" sm="6">
                                    <v-text-field label="特征码" v-model="activebill.signatureCode" readonly/>
                                </v-col>
                            </v-row>
                            <v-row v-if="permission.modifyPayment===true  && const_bill_status[activebill.status].unpaid===true"> 
                                <v-col />
                                <v-col cols="auto">
                                    <v-btn outlined @click.stop="comfirmPaymentDialog=true;" class="mx-2" color="primary">确认支付</v-btn>
                                    <v-btn outlined @click.stop="cancelPaymentDialog=true;" class="mx-2" color="deep-orange accent-2">关闭账单</v-btn>
                                </v-col>
                            </v-row>
                        </v-container>
                    </v-card-text>
                    <v-card-actions>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="notFoundDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text>
                        <p class="mt-6 d-flex justify-center">未找到该特征码所对应的激活中的账单</p>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="comfirmPaymentDialog" v-if="activebill!==null" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">
                            您确定 <span class="deep-orange--text accent-2">账单 {{activebill.billid}} 已经正确、完整、无误地完成了支付</span> 吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="comfirmPaymentDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" @click="completePayment();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="cancelPaymentDialog" v-if="activebill!==null" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">
                            您确定要 <span class="deep-orange--text accent-2">关闭 账单 {{activebill.billid}} </span> 吗？
                        </p>
                        <p class="deep-orange--text accent-2 mb-0">关闭账单将可能造成严重的错误</p>
                        <p class="primary--text my-0" v-if="const_bill_type[activebill.type]!==true">错误关闭注册帐单有可能导致用户无法完成缴费流程</p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="comfirmPaymentDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" @click="cancelPayment();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="notedialog.active" width="200">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text>
                        <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                        <p v-else class="d-flex justify-center">操作失败！</p>
                    </v-card-text>
                </v-card>
            </v-dialog>
        </template>
    </div>
</template>
<script>
import {const_bill_type,const_bill_status} from '@/utils/bill-type-status'
import mixin from '@/utils/mixins'
export default {
    name:"Payments-Management",
    mixins:[mixin],
    data:()=>({
        reg_search:null,
        wd_search:null,
        tab:null,
        activebill:null,
        activeBillDialog:false,
        searchSCDialog:null,
        notFoundDialog:null,
        comfirmPaymentDialog:null,
        cancelPaymentDialog:null,
        notedialog:{
            active:null,
            success:true,
        },
        
        sc:null,
        reg_bills:[],
        wd_bills:[],
        header:[
            {text:'账单号',value:'billid'},
            {text:'代表姓名',value:'uname'},
            {text:'类型',value:'type_hint'},
            {text:'金额',value:'amount'},
            {text:'状态',value:'status_hint'},
            {text:'特征码',value:'signatureCode'},
        ],
        permission:{
            modifyPayment:false,
        },
        const_bill_type,
        const_bill_status,
    }),
    methods:{
        getBills:function(){
            this.getRegBills();
            this.getWdBills();
            this.getPermission();
        },
        getPermission:function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/payment')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        getRegBills:function(){
            var vm=this;
            this.fetchGet('/api/payment/reg')
            .then(function(res){
                if(res.status===200){
                    res.json().then(async function(json){
                        await vm.initBillsHint(json);
                        vm.reg_bills=json;
                    })
                }
            })
        },
        getWdBills:function(){
            var vm=this;
            this.fetchGet('/api/payment/wdl')
            .then(function(res){
                if(res.status===200){
                    res.json().then(async function(json){
                        await vm.initBillsHint(json);
                        vm.wd_bills=json;
                    })
                }
            })
        },
        showBill:function(event){
            this.activebill=event;
            this.activebill.originalprice=this.amount;
            this.activeBillDialog=true;
        },
        initBillsHint:async function(bills){
            for(var i=0;i<bills.length;i++){
                var rbill=bills[i];
                var res=await this.fetchGet('/api/user/name/'+rbill.uid)
                if(res.status===200){
                    var json=await res.json()
                    rbill.uname=json.name;
                }
                rbill.type_hint=this.const_bill_type[rbill.type].hint;
                rbill.status_hint=this.const_bill_status[rbill.status].hint;
            }
        },
        openSearchSCDialog:function(){
            this.sc=null;
            this.activebill=null;
            this.searchSCDialog=true;
        },
        searchSC:async function(){
            this.searchSCDialog=false;
            var vm=this;
            var res=await vm.fetchGet('/api/payment/sc/'+this.sc);
            if(res.status===200){
                var json=await res.json();
                var resu=await vm.fetchGet('/api/user/name/'+json.uid)
                if(resu.status===200){
                    var ju=await resu.json()
                    json.uname=ju.name
                }
                json.type_hint=vm.const_bill_type[json.type].hint;
                json.status_hint=vm.const_bill_status[json.status].hint;
                vm.activebill=json;
                vm.activebill.originalprice=json.amount;
                vm.activeBillDialog=true;
            }
            else{
                vm.notFoundDialog=true;
            }
        },
        completePayment:async function(){
            var vm=this;
            var res=null;
            if(vm.activebill.amount===vm.activebill.originalprice)
                res=await vm.fetchPostJson('/api/payment/'+vm.activebill.billid,{})
            else
                res=await vm.fetchPutJson('/api/payment/'+vm.activebill.billid,vm.activebill.amount)
            if(res.status===204){
                vm.notedialog.success=true;
                vm.notedialog.active=true;
                vm.getBills();
                
            }
            else{
                vm.notedialog.success=false;
                vm.notedialog.active=true;
            }
            this.comfirmPaymentDialog=false;
        },
        cancelPayment:function(){
            var vm=this;
            vm.fetchDelete('/api/payment/'+vm.activebill.billid)
            .then(function(res){
                if(res.status===204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                    vm.getBills();
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.cancelPaymentDialog=false;
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getBills())
    },
    beforeRouteUpdate (to, from, next) {
        this.getBills();
        next()
    },
}
</script>