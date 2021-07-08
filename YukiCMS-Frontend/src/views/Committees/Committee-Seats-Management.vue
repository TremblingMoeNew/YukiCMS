<template>
    <div>
        <div class="text-h5 mx-6 mt-6">{{com!==null?com.name:"会场不存在"}} / 席位管理</div>
        <div v-if="permission.assignSeats!==true && permission.createSeats!==true">
            <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" >您没有查看的权限</div>
        </div>
        <template v-else>
            <div class="mx-lg-9 d-flex align-center">
                <v-tabs v-model="tab" right center-active>
                    <v-tab>未分配席位</v-tab>
                    <v-tab>已分配席位</v-tab>
                    <v-tab>未分配代表</v-tab>
                    <v-btn text class="grey--text darken-2 align-self-center" @click.stop="createSeatsDialog=true" v-if="permission.createSeats===true">创建席位</v-btn>
                </v-tabs>
                <v-divider />
            </div>
            <v-dialog v-model="createSeatsDialog" width="400">
                <v-card width="400">
                    <v-toolbar flat dense color="blue lighten-4">
                        <v-toolbar-title>创建席位</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-form class="mx-5">
                            <v-text-field label="席位名称" name="sname" id="name" v-model="newSeat"/>
                        </v-form>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn class="mx-2" color="blue lighten-4" :disabled="permission.createSeats!==true" @click="submitCreate();">创建</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="assignSeatsDialog" width="400">
                <v-card width="400">
                    <v-toolbar dense flat color="blue lighten-4">
                        <v-toolbar-title>分配席位</v-toolbar-title>
                        <v-spacer></v-spacer>
                    </v-toolbar>
                    <v-card-text>
                        <v-form class="mx-5 mt-6">
                            <v-select label="席位" :items="unassigned_seats" item-text="name" item-value="sid" v-model="activeSeat.sid"/>
                            <v-select label="代表" :items="unassigned_users" item-text="name" item-value="uid" v-model="activeSeat.uid"/>
                        </v-form>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn class="mx-2" color="blue lighten-4" :disabled="permission.assignSeats!==true" @click="submitAssign();">提交</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="notedialog.active" width="200">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text>
                        <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                        <p v-else class="d-flex justify-center">操作失败！</p>
                    </v-card-text>
                </v-card>
            </v-dialog>
            <v-dialog v-model="deleteSeatsDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">您确定要删除席位
                            <span  class="deep-orange--text accent-2"> {{activeSeat.name}} </span>
                            吗？
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="deleteSeatsDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="permission.createSeats!==true" @click="submitDelete();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-dialog v-model="unassignSeatsDialog" width="400">
                <v-card>
                    <v-card-title>提示</v-card-title>
                    <v-card-text style="text-align: center;">
                        <p class="mt-6">
                            您确定要撤销席位
                            <span  class="deep-orange--text accent-2">{{activeSeat.name}}</span>
                            的分配吗？
                        </p>
                        <p>
                            该席位当前被分配给代表
                            <span class="deep-orange--text accent-2">{{activeSeat.uname}}</span>
                        </p>
                    </v-card-text>
                    <v-card-actions>
                        <v-spacer />
                        <v-btn text @click="unassignSeatsDialog=false;">取消</v-btn>
                        <v-btn text color="deep-orange accent-2" :disabled="permission.assignSeats!==true" @click="submitUnassign();">确定</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-tabs-items v-model="tab" class="mx-lg-12 my-3">
                <v-tab-item>
                    <v-data-table :headers="seats_header" :items="unassigned_seats" @click:row="openAssignDialog" class="mx-12" calculate-widths :loading="loading" loading-text="加载中……"  item-key="sid">
                        <template v-slot:item.actions="{ item }">
                            <v-btn text color="primary" :disabled="permission.assignSeats!==true" @click.stop="openAssignDialog(item)">分配</v-btn>
                            <v-btn text color="deep-orange accent-2" :disabled="permission.createSeats!==true" @click.stop="openDeleteDialog(item)">删除</v-btn>
                        </template>

                    </v-data-table>
                </v-tab-item>
                <v-tab-item>
                    <v-data-table :headers="seats_header" :items="assigned_seats" @click:row="openUnassignDialog" class="mx-12" calculate-widths :loading="loading" loading-text="加载中……">
                        <template v-slot:item.actions="{ item }">
                            <v-btn text color="primary" :disabled="permission.assignSeats!==true" @click.stop="openUnassignDialog(item)">取消分配</v-btn>
                            <v-btn text color="deep-orange accent-2" :disabled="permission.createSeats!==true" @click.stop="openDeleteDialog(item)">删除</v-btn>
                        </template>
                    </v-data-table>
                </v-tab-item>
                <v-tab-item>
                    <v-data-table :headers="users_header" :items="unassigned_users" @click:row="openAssignDialog" class="mx-12" calculate-widths :loading="loading" loading-text="加载中……">
                        <template v-slot:item.actions="{ item }">
                            <v-btn text color="primary" :disabled="permission.assignSeats!==true" @click.stop="openUser(item)">转到</v-btn>
                            <v-btn text color="primary" :disabled="permission.assignSeats!==true" @click.stop="openAssignDialog(item)">分配</v-btn>
                        </template>
                    </v-data-table>
                </v-tab-item>

            </v-tabs-items>
        </template>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Seats-Management",
    mixins:[mixin],
    props:['cid'],
    data:()=>({
        tab:null,
        createSeatsDialog:null,
        assignSeatsDialog:null,
        unassignSeatsDialog:null,
        deleteSeatsDialog:null,
        newSeat:null,
        loading:false,
        notedialog:{
            active:null,
            success:true,
        },
        activeSeat:{
            
        },
        permission:{
            createSeats:false,
            assignSeats:false,
        },
        com:null,
        seats_header:[
            {text:'席位名',value:'name',align:'center'},
            {text:'席位状态',value:'status_str',align:'center'},
            {text:'代表姓名',value:'uname',align:'center'},
            {text:'',value:'actions',sortable:false,align:'center'}
        ],
        users_header:[
            {text:'代表姓名',value:'name',align:'center'},
            {text:'代表状态',value:'status_str',align:'center'},
            {text:'席位名',value:'sname',align:'center'},
            {text:'',value:'actions',sortable:false,align:'center'}
        ],
        unassigned_seats:[],
        assigned_seats:[],
        unassigned_users:[],
    }),
    watch:{
        com:function(ncom){
            if(ncom!==null){
                this.getPermission()
                this.getLists()
            }
        },
    },
    methods:{
        getCommittee:function(){
            var vm=this;
            this.fetchGet('/api/committee/c'+this.cid+'/brief')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.com=json;
                    })
                }
            })
        },
        getPermission:function() {
            var vm=this;
            this.fetchGet('/api/permission/vertify/committee/seat/'+this.cid)
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        vm.permission=json;
                    })
                }
            })
        },
        getLists:function(){
            this.getUnassignedSeats();
            this.getAssignedSeats();
            this.getUnassignedUsers();
        },
        getUnassignedSeats:function(){
            var vm=this;
            this.fetchGet('/api/seat/c'+this.cid+'/unassigned')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        for(var i=0;i<json.length;i++)
                        {
                            json[i].status_str='未分配'
                            json[i].uid=0
                            json[i].uname="尚未分配"
                        }
                        vm.unassigned_seats=json;
                    })
                }
            })
        },
        getAssignedSeats:function(){
            var vm=this;
            this.fetchGet('/api/seat/c'+this.cid+'/assigned')
            .then(function(res){
                if(res.status===200){
                    res.json().then(async function(json){
                        for(var i=0;i<json.length;i++){
                            json[i].status_str='已分配'
                            var r=await vm.fetchGet('/api/user/name/'+json[i].uid)
                            if(r.status===200){
                                var usr=await r.json()
                                json[i].uname=usr.name;
                            }
                        }
                        vm.assigned_seats=json;
                    })
                }
            })
        },
        getUnassignedUsers:function(){
            var vm=this;
            this.fetchGet('/api/committee/c'+this.cid+'/members/unassigned')
            .then(function(res){
                if(res.status===200){
                    res.json().then(function(json){
                        for(var i=0;i<json.length;i++)
                        {
                            json[i].status_str='未分配'
                            json[i].sname="尚未分配"
                        }
                        vm.unassigned_users=json;
                    })
                }
            })
        },
        submitCreate: function () {
            if(this.permission.createSeats!==true)return;
            var vm=this;
            vm.fetchPostJson('/api/seat',{
                cid:vm.com.cid,
                name:vm.newSeat
            }).then(function(res){
                if(res.status===201){
                    res.json().then(function(json){
                        json.status_str='未分配'
                        json.uname="尚未分配"
                        vm.unassigned_seats.push(json)
                        vm.notedialog.success=true;
                        vm.notedialog.active=true;
                    })
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.createSeatsDialog=false
        },
        submitAssign: function () {
            if(this.permission.assignSeats!==true)return;
            var vm=this;
            vm.fetchPostJson('/api/seat/s'+this.activeSeat.sid+'/u'+this.activeSeat.uid,{})
            .then(function(res){
                if(res.status===204){
                    vm.getLists();
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.assignSeatsDialog=false;
        },
        submitUnassign: function () {
            if(this.permission.assignSeats!==true)return;
            var vm=this;
            vm.fetchDelete('/api/seat/s'+this.activeSeat.sid+'/u'+this.activeSeat.uid)
            .then(function(res){
                if(res.status===204){
                    vm.getLists();
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.unassignSeatsDialog=false;
        },
        submitDelete: function(){
            if(this.permission.createSeats!==true)return;
            var vm=this;
            vm.fetchDelete('/api/seat/s'+this.activeSeat.sid)
            .then(function(res){
                if(res.status===204){
                    vm.getLists();
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.deleteSeatsDialog=false;
        },
        openCreateSeatsDialog:function(){
            this.newSeat=null;
            this.createSeatsDialog=true
        },
        openAssignDialog: function(event){
            if(this.permission.assignSeats!==true)return;
            this.activeSeat=event;
            this.assignSeatsDialog=true;
        },
        openUnassignDialog: function(event){
            if(this.permission.assignSeats!==true)return;
            this.activeSeat=event;
            this.unassignSeatsDialog=true;
        },
        openDeleteDialog: function(event){
            if(this.permission.createSeats!==true)return;
            this.activeSeat=event;
            this.deleteSeatsDialog=true;
        },
        openUser:function(item){
            this.$router.push({name:'User-Management',params:{uid:item.uid}})
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getCommittee())
    },
    beforeRouteUpdate (to, from, next) {
        this.getCommittee();
        next()
    },
}
</script>