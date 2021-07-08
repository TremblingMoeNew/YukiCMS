<template>
    <div class="d-flex align-center justify-center fill-height">
        <v-card class="elevation-3 mx-3" width="600">
            <v-toolbar dark flat color="primary" >
                <v-toolbar-title>用户登录</v-toolbar-title>
                <v-spacer></v-spacer>
                <router-link to="Register">
                    <v-btn text>注册</v-btn>
                </router-link>
            </v-toolbar>
            <v-card-text >
                <v-form class="mx-5">
                    <v-text-field label="邮箱" v-model="email" />
                    <v-text-field label="密码" type="password" v-model="password" @keydown.enter="login()"/>
                </v-form>
            </v-card-text>
            <p class="d-flex justify-center deep-orange--text accent-2" v-if="error_hint===true">用户名或密码错误</p>
            <v-card-actions>
                <v-btn class="mx-7 mb-3 mt-0" text color="primary" @click="$router.push({name:'Forget-Pass'})">忘记密码？</v-btn>
                <v-spacer />
                <v-btn class="mx-7 mb-3 mt-0" color="primary" @click="login()" :disabled="email===null || password === null">登录</v-btn>
            </v-card-actions>
        </v-card>
    </div>
</template>
<script>
export default {
    name:"Login",
    data:()=>({
        email:null,
        password:null,
        error_hint:null,
    }),
    methods:{
        login:async function(){
            if(this.email===null || this.password === null)return;
            this.error_hint=false;
            var res=await this.fetchLogin({
                    email:this.email,
                    password:this.password,
                });
            if(res.success===true){
                this.$store.commit('setjwt',res.token);
            }
            else{
                this.error_hint=true;
            }
        },
        fetchLogin:function(data){
            return (fetch(this.$store.state.backendUrl+'/api/user/login', {
                method: 'POST',
                body: JSON.stringify(data),
                headers: new Headers({
                    'Content-Type': 'application/json;charset=utf-8',
                }),
            })
            .then(res => res.json()))
        }
    }
}
</script>
