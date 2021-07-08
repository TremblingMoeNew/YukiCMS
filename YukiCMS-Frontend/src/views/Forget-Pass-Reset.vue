<template>
    <div class="d-flex align-center justify-center fill-height">
        <v-card class="elevation-3 mx-3" width="600">
            <v-toolbar dark flat color="primary" >
                <v-toolbar-title>重置密码</v-toolbar-title>
                <v-spacer></v-spacer>
            </v-toolbar>
            <template v-if="tokenava===true">
                <v-card-text >
                    <v-form class="mx-5">
                        <v-text-field label="新的密码" type="password" v-model="password" />
                        <v-text-field label="重复新密码" type="password" v-model="passwordrepeat"  @keydown.enter="reset()" />
                    </v-form>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn class="mx-7 mb-3 mt-0" color="primary" @click="reset()" :disabled=" password===null || passwordrepeat===null || password!==passwordrepeat">确认</v-btn>
                </v-card-actions>
            </template>
            <template v-else>
                <v-card-text>
                    <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" >
                        加载中……
                    </div>
                </v-card-text>
            </template>
        </v-card>
        <v-dialog v-model="success_hint" width="400" persistent>
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p class="d-flex justify-center green--text darken-1">密码重置成功！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-dialog v-model="expire_hint" width="400" persistent>
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p class="d-flex justify-center deep-orange--text accent-1">重置密码申请无效或已过期！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
    </div>
</template>
<script>
import lodash from 'lodash'
export default {
    name:"Forget-Pass-Reset",
    props:['token'],
    data:()=>({
        password:null,
        passwordrepeat:null,
        tokenava:false,
        success_hint:null,
        expire_hint:null,
    }),
    methods:{
        percheck:function(){
            var vm=this;
            fetch(this.$store.state.backendUrl+'/api/user/forgetpassword/'+this.token, {
                method: 'GET',
            }).then(function(res){
                if(res.status===204){
                    vm.tokenava=true;
                }else if(res.status===404){
                    vm.expire_hint=true;
                    vm.debounceJumpToForgetPass();
                }
            })
        },
        reset:async function(){
            var vm=this;
            this.fetchReset({
                    token:this.token,
                    npassword:this.password,
                }).then(function(res){
                    if(res.status===204){
                        vm.success_hint=true;
                        vm.debounceJumpToLogin();
                    }else if(res.status===404){
                        vm.expire_hint=true;
                        vm.debounceJumpToForgetPass();
                    }
                })
        },
        fetchReset:function(data){
            return fetch(this.$store.state.backendUrl+'/api/user/forgetpassword/reset', {
                method: 'POST',
                body: JSON.stringify(data),
                headers: new Headers({
                    'Content-Type': 'application/json;charset=utf-8',
                })
            })
        },
        jumpToLogin:function(){
            this.$router.push({name:'Login'})
        },
        jumpToForgetPass:function(){
            this.$router.push({name:'Forget-Pass'})
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.percheck())
    },
    beforeRouteUpdate (to, from, next) {
        this.percheck();
        next()
    },
    created:function(){
        this.debounceJumpToLogin=lodash.debounce(this.jumpToLogin, 3000)
        this.debounceJumpToForgetPass=lodash.debounce(this.jumpToForgetPass, 3000)
    }

}
</script>