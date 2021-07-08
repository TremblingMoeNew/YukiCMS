<template>
    <div>
        <div class="text-h5 mx-6 mt-6">用户管理</div>
        <template v-if="enabledPages.enableUserManagement!==true" >
            <v-divider class="mx-9 mt-6"/>
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto">
                您没有查看的权限
            </div>
        </template>
        <template v-else>
            <v-container>
                <v-row>
                    <v-col>
                        <v-divider/>
                    </v-col>
                </v-row>
                <v-row justify="center">
                    <v-col order="1" order-lg="3" lg="3" cols="12">
                        <v-container >
                            <v-row>
                                <v-col cols="12" sm="6" lg="12"> 
                                    <v-card class="mx-sm-12 mx-lg-0">
                                        <v-card-title>代表总概况</v-card-title>
                                        <v-card-text>
                                            <div class="ml-12">
                                                <p>注册人数：<span class="primary--text">{{counts.total}}</span></p>
                                                <p>支付人数：<span class="green--text darken-1">{{counts.paid}}</span></p>
                                                <p>住宿人数：<span class="teal--text accent-3">{{counts.ga}}</span></p>
                                                <p>退会人数：<span class="deep-orange--text accent-2">{{counts.withdrawed}}</span></p>
                                            </div>
                                        </v-card-text>
                                    </v-card>
                                </v-col>
                                <v-col cols="12" sm="6" lg="12"> 
                                    <v-card class="mx-sm-12 mx-lg-0" v-if="filterSelect!=='all'">
                                        <v-card-title>筛选代表概况</v-card-title>
                                        <v-card-subtitle>模式：{{selectedCounts.mode}}</v-card-subtitle>
                                        <v-card-text>
                                            <div class="ml-12" v-if="filterSelect==='withdrawed'">
                                                <p>退会人数：<span class="deep-orange--text accent-2">{{counts.withdrawed}}</span></p>
                                            </div>
                                            <div class="ml-12" v-else>
                                                <p>注册人数：<span class="primary--text">{{selectedCounts.total}}</span></p>
                                                <p>支付人数：<span class="green--text darken-1">{{selectedCounts.paid}}</span></p>
                                                <p>住宿人数：<span class="teal--text accent-3">{{selectedCounts.ga}}</span></p>
                                            </div>
                                        </v-card-text>
                                    </v-card>
                                </v-col>
                            </v-row>
                        </v-container>
                    </v-col>
                    <v-col order="2" lg="auto" cols="12">
                        <v-divider class="d-none d-lg-flex" vertical/>
                        <v-divider class="d-flex d-lg-none my-5"/> 
                    </v-col>
                    <v-col order="3" order-lg="1">
                        <div class="d-flex">
                            <v-menu offset-y :close-on-content-click="false">
                                <template v-slot:activator="{ on, attrs }">
                                    <v-btn color="primary" dark v-bind="attrs" v-on="on">筛选</v-btn>
                                </template>
                                <v-list dense>
                                    <v-radio-group dense v-model="filterSelect">
                                        <v-list-item link dense>
                                            <v-list-item-title>
                                                <v-radio value="all" label="显示全部"/>
                                            </v-list-item-title>
                                            <v-list-item-action/>
                                        </v-list-item>
                                        <v-list-item link dense v-if="comlist!==null">
                                            <v-list-item-title>
                                                <v-radio value="unapplied" label="未报名会场的"/>
                                            </v-list-item-title>
                                        </v-list-item>
                                        <v-list-item link dense v-if="comlist!==null">
                                            <v-list-item-title>
                                                <v-radio value="mutex" label="报名互斥会场的"/>
                                            </v-list-item-title>
                                        </v-list-item>
                                        <v-list-item link dense v-if="comlist!==null">
                                            <v-list-item-title>
                                                <v-radio value="unmutex" label="报名非互斥会场的"/>
                                            </v-list-item-title>
                                            <v-list-item-action/>
                                        </v-list-item>
                                        <v-list-item link dense v-if="comlist!==null">
                                            <v-list-item-title>
                                                <v-radio value="withdrawed" label="已退会的"/>
                                            </v-list-item-title>
                                        </v-list-item>
                                        <v-list-item link dense v-if="comlist!==null">
                                            <v-list-item-title>
                                                <v-radio value="bycom" label="按照会场筛选"/>
                                            </v-list-item-title>
                                        </v-list-item>
                                        <template v-if="comlist!==null && filterSelect==='bycom'">
                                            <v-list-item link class="d-flex py-0" v-for="(com,idx) in comlist" :key="idx">
                                                <v-list-item-title class="my-0 py-0">
                                                    <v-switch class="mx-3 my-0" dense v-model="selectedCom" :label="com.name" :value="com.cid"/>
                                                </v-list-item-title>
                                            </v-list-item>
                                        </template>
                                    </v-radio-group>
                                </v-list>
                            </v-menu>
                            <v-spacer/>
                            <v-text-field
                                v-model="search"
                                append-icon="mdi-magnify"
                                label="搜索"
                                single-line
                                hide-details
                            />
                        </div>
                        <v-data-table :headers="header" :items="users_list" calculate-widths :search="filterSelect" @dblclick:row="openUser" :custom-filter="userfilter" sort-by="uid"/>
                    </v-col>
                </v-row>
            </v-container>
        </template>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
import {const_ctype} from '@/utils/committee-type'
export default {
    name:"Users-Management",
    mixins:[mixin],
    data:()=>({
        search:null,
        counts:{
            total:0,
            paid:0,
            ga:0,
            withdrawed:0,
        },
        users_list:[], 
        header:[
            {text:'编号',value:'uid'},
            {text:'姓名',value:'name'},
            {text:'性别',value:'sex'},
            {text:'学校',value:'school'},
            {text:'团体报名',value:'isinSchoolGroup_hint'},
            {text:'住宿',value:'isGA_hint'},
            {text:'支付',value:'isRegPaid_hint'},
            {text:'退会',value:'isWithdrawaled_hint'}
        ],
        filterSelect:"all",
        comlist:null,
        selectedCom:[],
        selectedCounts:{
            total:0,
            paid:0,
            ga:0,
            mode:"",
        },
        const_ctype,
    }),
    watch:{
        filterSelect:function(){
            this.selectedCounts={
                total:0,
                paid:0,
                ga:0,
                mode:"",
            };
            var i=0;
            if(this.filterSelect==="unapplied"){
                this.selectedCounts.mode="未报名会场的"
                for(i=0;i<this.users_list.length;i++){
                    this.countUnapplied(this.users_list[i]);
                }
            }
            else if(this.filterSelect==='mutex'){
                this.selectedCounts.mode="报名互斥会场的"
                for(i=0;i<this.users_list.length;i++){
                    this.countMutex(this.users_list[i]);
                }
            }
            else if(this.filterSelect==='unmutex'){
                this.selectedCounts.mode="报名非互斥会场的"
                for(i=0;i<this.users_list.length;i++){
                    this.countUnmutex(this.users_list[i]);
                }
            }
            else if(this.filterSelect==='bycom'){
                this.selectedCounts.mode="按照会场筛选"
                for(i=0;i<this.users_list.length;i++){
                    this.countSelected(this.users_list[i]);
                }
            }
            else if(this.filterSelect==='withdrawed'){
                this.selectedCounts.mode="已退会的"
            }
        },
        selectedCom:function(){
            this.selectedCounts={
                total:0,
                paid:0,
                ga:0,
                mode:"按照会场筛选",
            };
            for(var i=0;i<this.users_list.length;i++){
                this.countSelected(this.users_list[i]);
            }
        }
    },
    methods:{
        getUsers:function(){
            var vm=this;
            vm.fetchGet('/api/user')
            .then(function(res){
                if(res.status===200){
                    res.json().then(async function(json){
                        vm.users_list=[];
                        vm.initializeUsersList(json,vm.users_list)
                        vm.getCommitteeList()
                    })
                }
            })
        },
        getCommitteeList:async function(){
            var vm=this;
            this.fetchGet('/api/committee')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.selectedCom=[];
                        for(var i=0;i<json.length;i++){
                            vm.selectedCom.push(json[i].cid);
                        }
                        vm.comlist=json;
                    })
                }
            })
        },
        initializeUsersList:function(users_list,tgrlst){
            for(var i=0;i<users_list.length;i++)
            {
                this.initializeUser(users_list[i],tgrlst);
            }
        },
        initializeUser:async function(usr,tgrlst){
            var vm=this;
            this.counts.total++;
            this.fetchGet('/api/user/committee/'+usr.uid)
            .then(function(res){
                if(res.status===200){
                res.json().then(async function(json){
                        usr.com=json;
                    })
                }
            })
            usr.isinSchoolGroup_hint=(usr.isinSchoolGroup===true?"是":"否");
            if(usr.isRegPaid===true){
                vm.counts.paid++;
                usr.isRegPaid_hint="已支付"
            }
            else usr.isRegPaid_hint="未支付";
            if(usr.isGA===true)
            {
                vm.counts.ga++;
                usr.isGA_hint="是";
            }
            else usr.isGA_hint="否";
            if(usr.isWithdrawaled===true)
            {
                vm.counts.withdrawed++;
                usr.withdrawed=true;
                usr.isWithdrawaled_hint="是";
            }
            else usr.isWithdrawaled_hint="否";
            tgrlst.push(usr)
        },
        countUnapplied:function(usr){
            if(!usr.com||usr.com.length!==0||usr.withdrawed)return;
            this.dealSelectCount(usr);
        },
        countMutex:function(usr){
            if(!usr.com || usr.com.length===0)return;
            var onmutex=false;
            for(var j=0;j<usr.com.length;j++){
                if(const_ctype[usr.com[j].ctype].Mutual)onmutex=true;
            }
            if(onmutex===true)this.dealSelectCount(usr);
        },
        countUnmutex:function(usr){
            if(!usr.com || usr.com.length===0)return;
            var unmutex=false;
            for(var j=0;j<usr.com.length;j++){
                if(!const_ctype[usr.com[j].ctype].Mutual)unmutex=true;
            }
            if(unmutex===true)this.dealSelectCount(usr);
        },
        countSelected:function(usr){
            if(!usr.com)return;
            for(var k=0;k<usr.com.length;k++){
                if(this.selectedCom.indexOf(usr.com[k].cid)!==-1){
                    this.dealSelectCount(usr);
                    return;
                }
            }
        },
        dealSelectCount:function(usr){
            this.selectedCounts.total++;
            if(usr.isRegPaid)this.selectedCounts.paid++;
            if(usr.isGA)this.selectedCounts.ga++;
        },
        userfilter:function(value, search, item){
            var contentfilter=!this.search || value != null &&this.search != null &&value.toString().indexOf(this.search) !== -1;
            if(search==="all")return true&&contentfilter;
            else if(search==="unapplied")return (item.com&&item.com.length===0&&!item.withdrawed)&&contentfilter;
            else if(search==="mutex"){
                if(!item.com)return false;
                var onmutex=false;
                for(var i=0;i<item.com.length;i++){
                    if(const_ctype[item.com[i].ctype].Mutual)onmutex=true;
                }
                return onmutex&&contentfilter;
            }
            else if(search==="unmutex"){
                if(!item.com)return false;
                var unmutex=false;
                for(var j=0;j<item.com.length;j++){
                    if(!const_ctype[item.com[j].ctype].Mutual)unmutex=true;
                }
                return unmutex&&contentfilter;
            }
            else if(search==="bycom"){
                if(!item.com)return false;
                for(var k=0;k<item.com.length;k++){
                    if(this.selectedCom.indexOf(item.com[k].cid)!==-1 && contentfilter===true)return true;
                }
                return false;
            }
            else if(search==='withdrawed'){
                return item.withdrawed&&contentfilter;
            }
        },
        openUser:function(_,{item}){
            this.$router.push({name:'User-Management',params:{uid:item.uid}})
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getUsers())
    },
    beforeRouteUpdate (to, from, next) {
        this.getUsers();
        next()
    },
}
</script>