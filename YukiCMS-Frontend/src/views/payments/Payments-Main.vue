<template>
    <div>
        <div class="text-h5 mx-6 my-6">账单</div>
        <v-divider class="mx-9"/>
        <v-container>
            <v-row class="justify-center">
                <v-col cols="12" md="8" sm="10">
                    <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" v-if="bills===null || bills.length===0">
                        您暂无需要支付的账单
                    </div>
                    <v-card v-for="(bill,idx) of bills" :key="idx" class="my-2">
                        <v-card-title>{{const_bill_type[bill.type].hint}} / {{const_bill_status[bill.status].hint}}</v-card-title>
                        <v-card-text>
                            <div>
                                <p class="mx-12 text-subtitle-1">金额: {{bill.amount}} 元</p>
                                <p class="mx-12 text-subtitle-1">特征码:<span class="deep-orange--text accent-1"> {{bill.signatureCode}}</span></p>
                                <p class="d-flex justify-center deep-orange--text accent-2" v-if="const_bill_type[bill.type].isRefund!==true && const_bill_status[bill.status].unpaid===true">请在支付时备注账单特征码</p>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>
        </v-container>

    </div>
</template>
<script>
import {const_bill_type,const_bill_status} from '@/utils/bill-type-status'
import mixin from '@/utils/mixins'
export default {
    name:"Payments-Main",
    mixins:[mixin], 
    data:()=>({
        bills:[],
        const_bill_type,
        const_bill_status,
    }),
    methods:{
        getBills:function(){
            var vm=this;
            this.fetchGet('/api/payment')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.bills=json;
                    })
                }
            })
        },
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