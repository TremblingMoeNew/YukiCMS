<template>
    <div>
        <div class="text-h5 mx-6 my-6">个人信息</div>
        <v-divider class="mx-9"/>
        <div class="mx-2">
            <v-container >
                <v-row> 
                    <v-col sm="4" cols="12">
                        <v-text-field label="姓名" v-model="loginUsr.name" disabled/>
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-select
                            :items="['男','女']" 
                            label="性别"
                            v-model="info.sex"
                            disabled
                        /> 
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-text-field label="邮箱" v-model="loginUsr.email" disabled/>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col md="4" cols="12">
                        <v-select 
                            :items="[{hint:'身份证',value:0},{hint:'其它',value:1}]" 
                            item-text="hint" item-value="value" 
                            label="证件类型" 
                            v-model="info.residentIdType"
                            readonly 
                        /> 
                    </v-col>
                    <v-col sm="6" md="4" cols="12">
                        <v-text-field label="证件号" v-model="info.residentId" readonly/>
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-dialog width="500" >
                            <template v-slot:activator="{ on, attrs }">
                                <v-text-field label="出生日期" v-model="info.birthday"  v-bind="attrs" v-on="on" readonly/>
                            </template>
                            <v-date-picker name="" id="" v-model="info.birthday"/>
                        </v-dialog>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col sm="3" cols="12">
                        <v-switch id="foreignerSelect" label="我的地址在中国境内" class="mx-3" v-model="isDomestic"/>
                    </v-col>
                    <template v-if="isDomestic">
                        <v-col sm="1" cols="auto"/>
                        <v-col sm="4" cols="12">
                        <v-select
                            :items="province_list"
                            item-text="hint" item-value="value"
                            label="省份" name="province_dom" id="province_dom" v-model="province_index"
                        />
                        </v-col>
                        <v-col sm="4" cols="12">
                        <v-select
                            :items="city_list[province_index]"
                            label="城市" name="city_dom" id="city_dom" v-model="city_dom"
                        />
                        </v-col>
                    </template>
                    <template v-else>
                        <v-col sm="3" cols="12">
                        <v-text-field label="国家" name="state_for" id="state_for" v-model="state_for" />
                        </v-col>
                        <v-col sm="3" cols="12">
                        <v-text-field label="省级行政区" name="province_for" id="province_for" v-model="province_for"/>
                        </v-col>
                        <v-col sm="3" cols="12">
                        <v-text-field label="城市" name="city_for" id="city_for" v-model="city_for"/>
                        </v-col>
                    </template>
                </v-row>
                <v-row>
                    <v-col sm="8" cols="12">
                        <v-text-field label="学校" name="school" id="school" v-model="info.school" />
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-switch label="团体报名" v-model="info.isinSchoolGroup"/>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col sm="4" cols="12">
                        <v-text-field label="电话号码" name="phoneNumber" id="phoneNumber" v-model="info.phoneNumber" />
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-text-field label="QQ号" name="qqNumber" id="qqNumber" v-model="info.qqNumber" />
                    </v-col>
                    <v-col sm="4" cols="12">
                        <v-text-field label="微信号" name="wechatNumber" id="wechatNumber" v-model="info.wechatNumber" />
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <v-textarea label="会议经历" name="cv" id="cv" v-model="info.cv" outlined shaped auto-grow/>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col/>
                    <v-col cols="auto">
                        <v-spacer/>
                        <v-btn outlined @click="submitInfo()" :disabled="withdrawStatus.withdrawed===true">保存</v-btn>
                    </v-col>
                </v-row>
            </v-container>
            <v-divider class="mx-5 my-5"/>
            <v-container>
                <v-row>
                    <v-col class="text-h6">
                        修改密码
                    </v-col>
                </v-row>
                <v-row>
                    <v-col />
                    <v-col cols="12" md="6">
                        <v-text-field v-model="password" type="password" label="新密码"/>
                        <v-text-field v-model="passwordRepeat" type="password" label="重复新密码"/>
                    </v-col>
                    <v-col />
                </v-row>
                <v-row>
                    <v-col/>
                    <v-col cols="auto">
                        <v-spacer/>
                        <v-btn @click="submitResetPassword()" :disabled="password===null||password===''||password!==passwordRepeat || withdrawStatus.withdrawed===true" outlined>提交</v-btn>
                    </v-col>
                </v-row>
            </v-container>
            <template v-if="withdrawStatus.canWithdraw===true && enabledPages.withdrawed!==true">
                <v-divider class="mx-5 my-5"/>
                
                <v-container >
                    <v-row>
                        <v-col>
                            <div class="my-12"/>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col/>
                        <v-col cols="auto">
                            <v-btn outlined color="deep-orange accent-2" @click="openWithdrawDialog()">退会</v-btn>
                        </v-col>
                        <v-col/>
                    </v-row>
                    <v-dialog v-model="withdrawDialog" width="400">
                        <v-card>
                            <v-card-title>提示</v-card-title>
                            <v-card-text>
                                <div class="d-flex justify-center">
                                    <p class="mt-6">您真的确定要
                                        <span class="deep-orange--text accent-2"> 退会 </span>
                                        吗？
                                    </p>
                                </div>
                                <div class="d-flex justify-center">
                                    <p class="deep-orange--text accent-2">
                                        此操作对您不可逆。一旦确认，您将无法自行取消。
                                    </p>
                                </div>
                            </v-card-text>
                            <v-card-actions>
                                <v-spacer />
                                <v-btn text @click="withdrawDialog=false;">取消</v-btn>
                                <v-btn text color="deep-orange accent-2" :disabled="withdrawEnabled!==true" @click="submitWithdraw();">确定</v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                </v-container>
            </template>
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
import {province_list,city_list} from '@/utils/provcitys'
import mixin from '@/utils/mixins'
import lodash from 'lodash'
export default {
    name:"User-Info-Edit",
    mixins:[mixin],
    data:()=>({
        info:null,
        notedialog:{
            active:null,
            success:true,
        },
        isDomestic:null,
        state_for:null,
        province_index:null,
        province_for:null,
        city_dom:null,
        city_for:null,
        password:null,
        passwordRepeat:null,
        withdrawDialog:null,
        withdrawEnabled:false,
        withdrawStatus:{
            withdrawed:false,
            canWithdraw:false,
        },
        province_list,
        city_list
    }),
    computed: {
        province_dom:function(){return this.province_index===null?null:this.province_list[this.province_index].hint},
        state:function(){return this.isDomestic===true?"中国":this.state_for},
        province:function(){return this.isDomestic===true?this.province_dom:this.province_for},
        city:function(){return this.isDomestic===true?this.city_dom:this.city_for}
    },
    methods:{
        getInfo:function(){
            var vm=this;
            this.fetchGet('/api/user/info')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                    vm.info=json;
                })
            }
        })
        },
        initAddressAndDate:function(){
            this.info.birthday=this.info.birthday.split(" ")[0];
            if(this.info.state==="中国")
            {
                this.isDomestic=true;
                for(var i=0;i<this.province_list.length;i++)
                {
                    if(this.province_list[i].hint===this.info.province)this.province_index=i;
                }
                this.city_dom=this.info.city;
            }
            else
            {
                this.state_for=this.info.state;
                this.province_for=this.info.province;
                this.city_for=this.info.city;
            }
        },
        submitInfo:function(){
            if(this.isDomestic===true)
            {
                this.info.city=this.city_dom;
                this.info.state="中国";
                this.info.province=this.province_list[this.province_index].hint;
            }
            else
            {
                this.info.state=this.state_for;
                this.info.province=this.province_for;
                this.info.city=this.city_for;
            }
            var vm=this;
            this.fetchPutJson('/api/user/info',this.info)
                .then(function(res){
                    if(res.status===200){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    }
                    else{
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
        },
        submitResetPassword:function(){
            if(this.password===this.passwordRepeat){
                var vm=this;
                vm.fetchPutJson('/api/user/password',{
                    npassword:vm.password
                }).then(function(res){
                    if(res.status===200){
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
        getWithdrawPermission:async function(){
            var vm=this;
            this.fetchGet('/api/permission/vertify/withdraw/'+this.loginUsr.uid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.withdrawStatus=json;
                    })
                }
            })
        },
        openWithdrawDialog:function(){
            this.withdrawEnabled=false;
            this.withdrawDialog=true;
            this.debounceEnableWithdrawSubmit();
        },
        enableWithdrawSubmit:function(){
            this.withdrawEnabled=true;
        },
        submitWithdraw:function(){
            var vm=this;
            this.fetchPostJson('/api/user/withdraw',{})
                .then(function(res){
                    if(res.status===204){
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                        vm.$emit('reload');
                    }
                    else{
                        vm.notedialog.success=false;
                        vm.notedialog.active=true;
                    }
                })
            this.withdrawDialog=false;
        }
    },
    watch:{
        info:function(){
            this.initAddressAndDate();
            this.getWithdrawPermission();
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getInfo())
    },
    beforeRouteUpdate (to, from, next) {
        this.getInfo();
        next()
    },
    created(){
        this.debounceEnableWithdrawSubmit=lodash.debounce(this.enableWithdrawSubmit, 5000)
    }
}
</script>