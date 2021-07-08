<template>
    <div>
        <div class="text-h5 mx-6 my-6">首页</div>
        <v-divider class="mx-9"/>
        <v-container>
            <v-row>
                <v-col order="1" order-md="4" md="4" cols="12">
                    <v-card class="my-2">
                        <v-card-title>概况</v-card-title>
                        <v-card-text>
                            <div v-if="general!==null" class="ml-12">
                                <template v-if="general.isWithdrawed===true">
                                    <p>状态：已选择退会</p>
                                    <p>退会进度：{{wdRefunding===true?'正在退款中':'退会已完成'}}</p>
                                </template>
                                <template v-else>
                                    <p>状态：{{general.isRegPaid===true?'已支付会费':'未支付会费'}}</p>
                                    <p>是否住宿：{{general.isGA===true?'是':'否'}}</p>
                                    <p>加入会场数：{{general.comCount}}</p>
                                    <p>待完成的学测数：{{general.activeReviewsCount}}</p>
                                    <p>席位数：{{general.seatsCount}}</p>
                                    <p>住宿分配：{{general.isAccAssigned===true?'已分配':'未分配'}}</p>
                                    <!-- Hardcoded Notifications for QQ Grp Nums -->
                                    <v-divider class="mr-12 mb-3" v-if="general.isShowDelGrpNum===true" />
                                    <p v-if="general.isShowDelGrpNum===true" class="deep-orange--text accent-2">请及时加入代表群</p>
                                    <p v-if="general.isShowDelGrpNum===true" class="deep-orange--text accent-2">代表总群：603617150</p>
                                    <p v-if="general.isShowGAGrpNum===true" class="deep-orange--text accent-2">GA2 会场群：1073089790</p>
                                    <p v-if="general.isShowXCGrpNum===true" class="deep-orange--text accent-2">X市 会场群：703138027</p>
                                    <p v-if="general.isShowWHOGrpNum===true" class="deep-orange--text accent-2">WHO 会场群：1154665908</p>
                                </template>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
                <v-col order="2" md="auto" cols="12">
                    <v-divider class="d-none d-lg-flex" vertical/>
                    <v-divider class="d-flex d-lg-none my-5"/> 
                </v-col>
                <v-col order="3" order-md="1">
                    <template v-if="general!==null && general.isWithdrawed===true">
                        <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" >
                            您已选择退会
                        </div>
                    </template>
                    <template v-else>
                        <v-card class="my-4">
                            <v-card-title>会场</v-card-title>
                            <v-card-text class="d-flex">
                                <v-container class="ml-md-6 ml-lg-12">
                                    <v-row v-if="coms===null || coms.length===0">
                                        <v-col>
                                            <div class="d-flex justify-center grey--text lighten-1 text-h6 align-self-auto" >
                                                您尚未加入任何会场
                                            </div>
                                        </v-col>
                                    </v-row>
                                    <v-row>
                                        <template v-for="(com,idx) of coms">
                                            <v-col md="5" :key="idx">
                                                <p class="text-subtitle-1 mb-0">{{com.name}}</p>
                                                    <div class="mx-12 mx-md-6 mx-lg-12">   
                                                        <p class="my-0">席位：{{com.seat}}</p>
                                                        <p class="my-0">进行中的学测数：{{com.reviewsCount}}</p>
                                                        <p class="my-0">{{com.paymentEnabled===true?'缴费中':'未开启缴费'}}</p>
                                                        <p class="my-0" v-if="com.price!==0">会费：{{com.price}}元</p>
                                                        <p class="my-0">截止日期：{{com.applyDDL}}</p>
                                                    </div>
                                            </v-col>
                                            <v-col md="auto" cols="12" :key="idx+100">
                                                <v-divider class="d-none d-lg-flex" vertical v-if="idx%2===0"/>
                                                <v-divider class="d-flex d-lg-none my-5" v-if="idx+1!==coms.length"/> 
                                            </v-col>
                                        </template>
                                    </v-row>
                                </v-container>
                            </v-card-text>
                        </v-card>
                        <v-card class="my-4">
                            <v-card-title>住宿 / 账单</v-card-title>
                            <v-card-text>
                                <v-container class="ml-md-6 ml-lg-12">
                                    <v-row>
                                        <v-col md="5" cols="12">
                                            <p class="text-subtitle-1 mb-0">住宿</p>
                                                <div v-if="general!==null" class="mx-12 mx-md-6 mx-lg-12">   
                                                    <p class="my-0">{{general.isGA===true?'选择住宿':'未选择住宿'}}</p>
                                                    <template v-if="general.isGA===true">
                                                        <p class="my-0">延宿天数：{{general.accExtendedDays}}</p>
                                                        <p class="my-0">住宿费：{{accprice}}元</p>
                                                    </template>
                                                </div>
                                        </v-col>
                                        <v-col md="auto" cols="12">
                                            <v-divider class="d-none d-lg-flex" vertical/>
                                            <v-divider class="d-flex d-lg-none my-5"/> 
                                        </v-col>
                                        <v-col md="5" cols="12">
                                            <p class="text-subtitle-1 mb-0">账单</p>
                                                <div class="mx-12 mx-md-6 mx-lg-12">   
                                                    <p class="my-0">未支付的账单数：{{billsCount}}</p>
                                                </div>
                                        </v-col>
                                    </v-row>
                                </v-container>
                            </v-card-text>
                        </v-card>
                    </template>
                </v-col>
            </v-row>
        </v-container>
    </div>
</template>

<script>
import mixin from '@/utils/mixins'
export default {
    name: 'Home',
    mixins:[mixin],
    data:()=>({
        coms:[],
        general:null,
        billsCount:0,
        wdRefunding:false,
        settings:null,
        isWithdrawed:true,
        // Hardcoded Notifications for QQ Grp Nums
        isShowDelGrpNum:false,
        isShowGAGrpNum:false,
        isShowGerGrpNum:false,
        isShowXCGrpNum:false,
        isShowWHOGrpNum:false,
        isShowObGrpNum:false,
    }),
    computed:{
        accprice:function(){
            if(this.general===null||this.settings===null)return 0;
            if(this.general.isGA){
                return this.settings.standardAccPrice+this.general.accExtendedDays*this.settings.extendedAccDailyPrice
            }
            return 0;
        }
    },
    methods:{
        getGeneral:async function(){
            var vm=this;
            this.fetchGet('/api/user/home/general')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.general=json;
                    })
                }
            })
        },
        getComs:async function(){
            var vm=this;
            this.fetchGet('/api/user/committee/')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        for(var i=0;i<json.length;i++){
                            var com=json[i]
                            com.seat="尚未分配";
                            com.reviewsCount=0;
                            com.paymentEnabled=false;
                            com.price=0;
                            com.refund=0;
                            com.applyDDL=com.applyDDL.split(" ")[0];
                            vm.fillcom(com);
                        }
                        vm.coms=json;
                    })
                }
            })
        },
        fillcom:function(com){
            var vm=this;
            // Hardcoded Notifications for QQ Grp Nums
            switch(com.cid){
                case 2:
                    vm.general.isShowDelGrpNum=true;
                    vm.general.isShowGAGrpNum=true;
                    break;
                case 4:
                    vm.general.isShowDelGrpNum=true;
                    vm.general.isShowGerGrpNum=true;
                    break;
                case 6:
                    vm.general.isShowDelGrpNum=true;
                    vm.general.isShowXCGrpNum=true;
                    break;
                case 8:
                    vm.general.isShowDelGrpNum=true;
                    vm.general.isShowWHOGrpNum=true;
                    break;
                case 14:
                    vm.general.isShowDelGrpNum=true;
                    vm.general.isShowObGrpNum=true;
                    break;
            }

            vm.fetchGet('/api/seat/u'+vm.loginUsr.uid+'/c'+com.cid)
            .then(function(ress){
                if(ress.status===200){
                    ress.json().then(function(jsons){
                        com.seat=jsons.name;
                    })
                }
            })
            vm.fetchGet('/api/review/c'+com.cid+'/self/active')
            .then(function(resr){
                if(resr.status===200){
                    resr.json().then(function(jsonr){
                        com.reviewsCount=jsonr.length;
                    })
                }
            })
            vm.fetchGet('/api/committee/settings/payments/'+com.cid)
            .then(function(resc){
                if(resc.status===200){
                    resc.json().then(function(jsonc){
                        com.paymentEnabled=jsonc.paymentEnabled;
                        com.price=jsonc.price;
                        com.refund=jsonc.refund;
                    })
                }
            })
        },
        getBillsCount:async function(){
            var vm=this;
            this.fetchGet('/api/payment/active')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.billsCount=json.length;
                    })
                }
            })
        },
        getSettings:function(){
            var vm=this;
            this.fetchGet('/api/global/settings')
            .then(function(res){
                if(res.status==200){
                    res.json().then(function(json){
                        vm.settings=json.settings;
                    })
                }
            })
        },
        initialize:function(){
            this.getGeneral();
            this.getComs();
            this.getBillsCount();
            this.getSettings();
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.initialize())
    },
    beforeRouteUpdate (to, from, next) {
        this.initialize();
        next()
    }
}
</script>
