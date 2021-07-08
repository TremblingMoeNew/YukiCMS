<template>
    <div class="d-flex align-center justify-center fill-height">
        <v-card class="elevation-3 mx-3" width="600">
            <v-toolbar dark flat color="primary" >
                <v-toolbar-title>忘记密码</v-toolbar-title>
                <v-spacer></v-spacer>
                <router-link to="Login">
                    <v-btn text>登录</v-btn>
                </router-link>
            </v-toolbar>
            <v-card-text >
                <v-form class="mx-5">
                    <v-text-field label="邮箱" v-model="email" @keydown.enter="send()" />
                </v-form>
            </v-card-text>
            <v-card-actions>
                <v-spacer />
                <v-btn class="mx-7 mb-3 mt-0" color="primary" @click="send()" :disabled="email===null">确认</v-btn>
            </v-card-actions>
        </v-card>
        <v-dialog v-model="send_hint" width="400">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p class="d-flex justify-center green--text darken-1">重设密码邮件已发送！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
    </div>
</template>
<script>
export default {
    name:"Forget-Pass",
    data:()=>({
        email:null,
        send_hint:false,
    }),
    methods:{
        send:async function(){
            if(this.email==null)return;
            this.send_hint=false;
            var vm=this;
            this.fetchSend({
                    email:this.email,
                }).then(function(){
                    vm.send_hint=true;
                })
            
        },
        fetchSend:function(data){
            return fetch(this.$store.state.backendUrl+'/api/user/forgetpassword', {
                method: 'POST',
                body: JSON.stringify(data),
                headers: new Headers({
                    'Content-Type': 'application/json;charset=utf-8',
                })
            })
        }
    }
}
</script>