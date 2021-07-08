<template>
    <div>
        <div class="text-h5 mx-6 my-6">会场申请</div>
        <v-divider class="mx-9"/>
        <div class="d-flex align-center justify-space-around flex-wrap">
            <template v-if="appilableCommittees===null || appilableCommittees.length===0">
                <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" >
                    您暂无可以申请加入的会场
                </div>
            </template>
            <template v-else>
                <v-dialog width="500" v-for="committee of appilableCommittees" :key="committee.cid" v-model="committee.dialog"> 
                    <template v-slot:activator="{ on, attrs }">
                        <v-card class="elevation-3 mx-5 my-5" width="500" v-bind="attrs" v-on="on">
                            <v-card-title >{{committee.name}}</v-card-title>
                            <v-card-subtitle class="deep-orange--text accent-2" v-if="committee.ctype==0">互斥性会场</v-card-subtitle>
                            <v-card-subtitle class="green--text darken-1" v-else-if="committee.ctype==1">非互斥会场</v-card-subtitle>
                                <v-card-subtitle class="grey--text darken-1" v-else-if="committee.ctype==3">申请已截止</v-card-subtitle>
                            <v-divider class="mx-5 my-0"/>
                            <v-card-text>
                                <pre class="text-body-2 mx-3" style="white-space: pre-wrap; word-wrap: break-word;">{{committee.cdesc}}</pre>
                            </v-card-text>
                            <v-card-subtitle>报名截止时间: {{committee.applyDDL}}</v-card-subtitle>
                        </v-card>
                    </template>
                    <v-card>
                        <v-card-title class="headline">警告</v-card-title>
                        <v-card-text class="text-body-1">
                            <template v-if="committee.ctype===3">
                                <p class="mx-2">“{{committee.name}}” 会场
                                    <span v-if="committee.ctype===3" class="grey--text accent-2">已经停止报名</span>
                                </p>
                            </template>
                            <template v-else>
                                <p class="mx-2">
                                    “{{committee.name}}” 会场属于
                                    <span v-if="committee.ctype==0" class="deep-orange--text accent-2">互斥性会场</span>
                                    <span v-else-if="committee.ctype==1" class="green--text darken-1">非互斥会场</span>
                                </p>
                                <p class="mx-2" v-if="committee.ctype==0">您仅能申请加入一个此类会场，且无法自行退出并重新申请</p>
                                <p class="mx-2" v-else-if="committee.ctype==1">此类会场通常为会议组织团队或学术团队的申请通道</p>
                                <p class="mx-2">您确定要继续吗？</p>
                            </template>
                        </v-card-text>
                        <v-divider></v-divider>
                        <v-card-actions>
                        <v-spacer></v-spacer>
                            <template v-if="committee.ctype!==3">
                                <v-btn color="orange lighten-1" text  @click="committee.dialog=false">取消</v-btn>
                                <v-btn color="primary" text  @click="submitApply(committee);committee.dialog=false">继续</v-btn>
                            </template>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
                <v-dialog width="500" v-model="resultDialog.show" @click:outside="onHintClose()">
                    <v-card>
                        <v-card-title>提示</v-card-title>
                        <v-card-text class="text-body-1">
                            <p class="d-flex justify-center" v-if="resultDialog.success">申请成功！</p>
                            <p class="d-flex justify-center" v-else>申请失败！</p>
                        </v-card-text>
                    </v-card>
                </v-dialog>

            </template>
            <v-dialog width="500" :value="afterRegister" persistent>
                <v-card>
                    <v-card-title>报名提示</v-card-title>
                    <v-card-text>
                        <div class="d-flex justify-center">
                            <p class="deep-orange--text accent-2">
                                请不要急于关闭窗口，注册并非您的报名的最后一步。
                            </p>
                        </div>
                        <div class="d-flex justify-center">
                            <p>
                                刚刚完成注册的您，当前尚未加入任何会场。
                            </p>
                        </div>
                        <div class="d-flex justify-center">
                            <p>
                                请您于新转到的“会场申请”页面中点击相应会场进行报名。
                            </p>
                        </div>
                        <div class="d-flex justify-center">
                            <p>
                                您也可以稍后从导航菜单中选择“会场——会场申请”重新访问。
                            </p>
                        </div>
                        <div class="d-flex justify-center">
                            <p class="grey--text ">
                                （移动端可点击左上角按钮打开导航菜单）
                            </p>
                        </div>
                        <div class="d-flex justify-center">
                            <p>
                                报名会场后，您仍需要登录本系统以完成学测、设置住宿等。
                            </p>
                        </div>
                        <div class="d-flex justify-center">
                            <p class="grey--text ">
                                （本提示将于十秒后自动关闭）
                            </p>
                        </div>
                    </v-card-text>
                </v-card>
            </v-dialog>
        </div>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
import lodash from 'lodash'
export default {
    name:"Committee-Apply",
    mixins:[mixin],
    data:()=>({
        resultDialog:{
            show:false,
            success:true,
            com:null,
        },
        appilableCommittees:null,
    }),
    computed:{
        afterRegister:function(){
            return this.$store.state.afterRegister;
        },
    },
    watch:{
        appilableCommittees:function(ncom){
            if(ncom!==null){
                this.initComStr();
            }
        },
    },
    methods: {
        getCommittees:function(){
            var vm=this;
            this.fetchGet('/api/committee/available')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.appilableCommittees=json;
                    })
                }
            })
        },
        initComStr:function(){
            for(var i=0;i<this.appilableCommittees.length;i++){
                var com=this.appilableCommittees[i];
                com.applyDDL=com.applyDDL.split(' ')[0];
            }
        },
        submitApply: function(committee){
            var vm=this;
            vm.fetchPostJson('/api/committee/c'+committee.cid,{})
            .then(function(res){
                if(res.status===204){
                    vm.resultDialog.com=committee;
                    vm.resultDialog.success=true;
                    vm.resultDialog.show=true;

                }
                else{
                    vm.resultDialog.com=null;
                    vm.resultDialog.success=false;
                    vm.resultDialog.show=true;
                }
            })
        },
        init:function(){
            this.debounceCloseHint();
            this.getCommittees();
        },
        closeHint:function(){
            this.$store.commit('arHintShowed');
        },
        onHintClose:function(){
            var vm=this;
            if(vm.resultDialog.success===true){
                vm.getCommittees()
                vm.$emit('reload');
            }
        },
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.init())
    },
    beforeRouteUpdate (to, from, next) {
        this.init();
        next()
    },
    created:function(){
        this.debounceCloseHint=lodash.debounce(this.closeHint,10000);
    }
}
</script>