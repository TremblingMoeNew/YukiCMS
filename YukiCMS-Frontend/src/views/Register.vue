<template>
    <div class="d-flex align-center justify-center fill-height">
        <v-card class="elevation-3 mx-3" width="600">
            <v-toolbar dark flat color="primary" >
                <v-toolbar-title>用户注册</v-toolbar-title>
                <v-spacer></v-spacer>
                <router-link to="Login">
                    <v-btn text>登录</v-btn>
                </router-link>
            </v-toolbar>
            <v-card-text >
                <v-form class="mx-0 my-0 px-0 py-0">
                    <v-stepper v-model="stepCounter">
                        <v-stepper-header>
                            <v-stepper-step :complete="stepCounter > 1" step="1">基本信息</v-stepper-step>
                            <v-divider></v-divider>
                            <v-stepper-step :complete="stepCounter > 2" step="2">个人身份</v-stepper-step>
                            <v-divider></v-divider>
                            <v-stepper-step :complete="stepCounter > 3" step="3">地址信息</v-stepper-step>
                            <v-divider></v-divider>
                            <v-stepper-step step="4">联系方式</v-stepper-step>
                        </v-stepper-header>
                        <v-stepper-items>
                            <v-stepper-content step="1">
                                <v-text-field label="邮箱" name="email" id="email" v-model="email" :rules="[rules.required, rules.email]"/>
                                <v-text-field 
                                    label="密码" 
                                    name="password"
                                    id="password" 
                                    type="password" 
                                    v-model="password" 
                                    :rules="[rules.required]"
                                />
                                <v-text-field 
                                    label="重复密码" 
                                    name="password-repeat" 
                                    id="password-repeat" 
                                    type="password" 
                                    v-model="passwordRepeat"
                                    :rules="[rules.required]"
                                />
                            </v-stepper-content>
                            <v-stepper-content step="2">
                                <v-text-field label="姓名" name="name" id="name" v-model="name"/>
                                <v-select
                                    :items="['男','女']" 
                                    label="性别"
                                    name="sex"
                                    id="sex"
                                    v-model="sex"
                                    :rules="[rules.required]"
                                /> 
                                <v-select 
                                    :items="[{hint:'身份证',value:0},{hint:'其它',value:1}]" 
                                    item-text="hint" item-value="value" 
                                    label="证件类型" 
                                    name="residentIdType" 
                                    id="residentIdType" 
                                    v-model="residentIdType" readonly
                                /> 
                                <v-text-field label="证件号" name="residentId" id="residentId" v-model="residentId" counter="18" :rules="[rules.required,rules.ridcounter]"/>
                                <v-dialog width="500" >
                                    <template v-slot:activator="{ on, attrs }">
                                        <v-text-field label="出生日期" name="birthday" id="birthday" v-model="birthday"  v-bind="attrs" v-on="on" readonly/>
                                    </template>
                                    <v-date-picker name="" id="" v-model="birthday"/>
                                </v-dialog>
                            </v-stepper-content>
                            <v-stepper-content step="3">
                                <v-switch id="foreignerSelect" label="我的地址在中国境内" class="mx-3" v-model="isDomestic"/> 
                                <template v-if="isDomestic">
                                    <v-select
                                        :items="province_list"
                                        item-text="hint" item-value="value"
                                        label="省份" name="province_dom" id="province_dom" v-model="province_index"
                                        :rules="[rules.requiredInteger]"
                                    />
                                    <v-select
                                        :items="city_list[province_index]"
                                        label="城市" name="city_dom" id="city_dom" v-model="city_dom"
                                        :rules="[rules.required]"
                                    />
                                </template>
                                <template v-else>
                                    <v-text-field label="国家" name="state_for" id="state_for" v-model="state_for" :rules="[rules.required]"/>
                                    <v-text-field label="省级行政区" name="province_for" id="province_for" v-model="province_for" :rules="[rules.required]"/>
                                    <v-text-field label="城市" name="city_for" id="city_for" v-model="city_for" :rules="[rules.required]"/>
                                </template>
                                <v-text-field label="学校" name="school" id="school" v-model="school" />
                                <v-select
                                    :items="[{hint:'是',value:true},{hint:'否',value:false}]"
                                    item-text="hint" item-value="value"
                                    label="团体报名" name="isinSchoolGroup" id="isinSchoolGroup" v-model="isinSchoolGroup" :rules="[rules.requiredBool]"
                                />
                            </v-stepper-content>
                            <v-stepper-content step="4">
                                <v-text-field label="电话号码" name="phoneNumber" id="phoneNumber" v-model="phoneNumber" :rules="[rules.required]"/>
                                <v-text-field label="QQ号" name="qqNumber" id="qqNumber" v-model="qqNumber" />
                                <v-text-field label="微信号" name="wechatNumber" id="wechatNumber" v-model="wechatNumber" />
                                <v-textarea label="会议经历" name="cv" id="cv" v-model="cv" outlined shaped/>
                            </v-stepper-content>
                        </v-stepper-items>
                    </v-stepper>
                </v-form>
            </v-card-text>
            <p class="d-flex justify-center deep-orange--text accent-2" v-if="error_hint!==null">
                <template v-if="error_hint===1">该邮箱已被注册</template>
                <template v-if="error_hint===2">通知邮件发送失败 请检查邮箱正确性</template>
            </p>
            <v-card-actions>
                <v-spacer />
                <v-btn class=" mb-3 mt-0" v-if="stepCounter>1" @click="stepCounter-=1">上一步</v-btn>
                <v-btn class="mx-7 mb-3 mt-0" v-if="stepCounter<4" @click="stepCounter+=1" color="primary" :disabled="vertifyPage!==true">下一步</v-btn>
                <v-btn class="mx-7 mb-3 mt-0" v-if="stepCounter===4" color="primary" :disabled="vertifyPage!==true" @click="register()">注册</v-btn>
            </v-card-actions>
        </v-card>
    </div>
</template>
<script>
const pattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
import {province_list,city_list} from '../utils/provcitys'
export default {
    name:"Register",
    data: () => ({
        stepCounter:1,
        email:null,
        password:null,
        passwordRepeat:null,
        name:null,
        sex:null,
        residentIdType:0,
        residentId:null,
        birthday:null,
        isDomestic:true,
        state_for:null,
        province_index:null,
        province_for:null,
        city_dom:null,
        city_for:null,
        school:null,
        isinSchoolGroup:null,
        phoneNumber:null,
        qqNumber:null,
        wechatNumber:null,
        cv:null,
        error_hint:null,
        province_list,
        city_list,
        rules: {
            required: value => !!value || '该项为必填项',
            requiredInteger:value=>value!==null || '该项为必填项',
            requiredBool:value=> value===true||value===false || '该项为必填项',
            email: value => {
                const pattern = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
                return pattern.test(value) || '邮箱格式错误.'
            },
            ridcounter: value => value.length == 18 || '身份证号码应为18位',
        },
    }),
    computed: {
        form:()=>({
            password:this.password,
            passwordRepeat:this.passwordRepeat,
        }),
        province_dom:function(){return this.province_index===null?null:this.province_list[this.province_index].hint},
        state:function(){return this.isDomestic===true?"中国":this.state_for},
        province:function(){return this.isDomestic===true?this.province_dom:this.province_for},
        city:function(){return this.isDomestic===true?this.city_dom:this.city_for},
        vertifyPage:function(){
            switch(this.stepCounter){
                case 1:
                    return this.email!==null && this.password!==null && this.password===this.passwordRepeat && pattern.test(this.email)
                case 2:
                    return this.name!==null && this.sex!==null && this.residentIdType!==null && this.residentId!==null && this.birthday!==null && (this.residentIdType===1 || this.residentId.length===18)
                case 3:
                    return this.state!==null && this.province!==null && this.city!==null && this.school!==null && this.isinSchoolGroup!==null
                case 4:
                    return this.phoneNumber!==null
            }
            return false;
        }
    },
    methods:{
        register:async function(){
            if(this.vertifyPage!==true)return;
            this.error_hint=null;
            var res=await this.fetchRegister({
                    email:this.email,
                    password:this.password,
                    info:{
                        name:this.name,
                        sex:this.sex,
                        residentIdType:this.residentIdType,
                        residentId:this.residentId,
                        birthday:this.birthday,
                        state:this.state,
                        province:this.province,
                        city:this.city,
                        school:this.school,
                        isinSchoolGroup:this.isinSchoolGroup,
                        phoneNumber:this.phoneNumber,
                        qqNumber:this.qqNumber,
                        wechatNumber:this.wechatNumber,
                        cv:this.cv,
                    }
                });
            if(res.success===true){
                this.$store.commit('afterRegister');
                this.$store.commit('setjwt',res.token);
            }
            else{
                if(res.emailconflict===true){
                    this.error_hint=1;
                }
                else if(res.notifEmailSendFailed===true){
                    this.error_hint=2;
                }
            }
        },
        fetchRegister:function(data){
            return (fetch(this.$store.state.backendUrl+'/api/user', {
                method: 'POST',
                body: JSON.stringify(data),
                headers: new Headers({
                    'Content-Type': 'application/json;charset=utf-8',
                }),
            })
            .then(res => res.json()))
        },
        
    }
}
</script>