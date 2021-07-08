<template>
    <div>
        <div class="text-h5 mx-6 my-6">住宿设置</div>
        <v-divider class="mx-9"/>
        <div class="mx-2">
            <v-container >
                <v-row> 
                    <v-col cols="auto"  align-self="center">
                        <v-switch :disabled="true" label="是否住宿" v-model="accinfo.isGA"/>
                    </v-col>
                    <v-col v-if="accinfo.isPaid===true" align-self="center" class="deep-orange--text accent-2">
                        您已完成缴费 住宿时长修改已锁定
                    </v-col>
                </v-row>
                <v-row v-if="accinfo.isGA===true">
                    <v-col md="4" cols="12">
                        <v-text-field label="您期望的室友姓名" v-model="accinfo.appliedRoommateName"/>
                    </v-col>
                    <v-col md="4" sm="6" cols="12">
                        <v-slider label="提前住宿天数" min="0" max="2" ticks :tick-labels="['不提前','一日','二日']" v-model="accinfo.aheadDays" :disabled="accinfo.isPaid===true" />
                    </v-col>
                    <v-col md="4" sm="6" cols="12">
                        <v-slider label="延长住宿天数" min="0" max="2" ticks :tick-labels="['不延长','一日','二日']" v-model="accinfo.extendDays" :disabled="accinfo.isPaid===true" />
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                    </v-col>
                    <v-col cols="auto">
                        <v-btn outlined @click.stop="submitSave()" >保存</v-btn>
                    </v-col>
                </v-row>
                <template v-if="roommate!==null">
                    <v-row>
                        <v-col>
                            <v-divider class="mx-9 my-12"/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col/>
                        <v-col cols="10" sm="8" md="6">
                            <v-card outlined>
                                <template v-if="roommate.singleAcc===true">
                                    <v-card-text>
                                        <p class="text-h6 grey--text d-flex justify-center">单人住宿</p>
                                    </v-card-text>
                                </template>
                                <template v-else>
                                    <v-card-title>室友信息</v-card-title>
                                    <v-card-text class="d-flex justify-center">
                                        <div>
                                            <p>姓名：{{roommate.name}}</p>
                                            <p>性别：{{roommate.sex}}</p>
                                            <p>邮箱：{{roommate.email}}</p>
                                            <p>电话号码：{{roommate.phoneNumber}}</p>
                                            <p>QQ号：{{roommate.qqNumber}}</p>
                                            <p>微信号：{{roommate.wechatNumber}}</p>
                                            <p class="mb-0">所在会场：</p>
                                            <p class="d-flex justify-center my-0 primary--text" v-for="(com,idx) of roommate.confs" :key="idx">
                                                {{com.name}}
                                            </p>
                                        </div>
                                    </v-card-text>
                                </template>
                            </v-card>
                        </v-col>
                        <v-col/>
                    </v-row>
                </template>
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
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Accommodation-Main",
    mixins:[mixin],
    data:()=>({
        notedialog:{
            active:null,
            success:true,
        },
        accinfo:null,
        roommate:null,
    }),
    methods:{
        getInfo:function(){
            var vm=this;
            this.fetchGet('/api/user/paymentStatus')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.accinfo=json.accommodation;
                        vm.accinfo.isPaid=json.isRegPaid;
                    })
                }
            })
        },
        submitSave:function(){
            var vm=this;
            this.fetchPutJson('/api/user/acc',vm.accinfo)
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
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getInfo())
    },
    beforeRouteUpdate (to, from, next) {
        this.getInfo();
        next()
    }
}
</script>