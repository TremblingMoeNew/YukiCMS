<template>
    <div>
        <div class="text-h5 mx-6 my-6">权限列表</div>
        <v-divider class="mx-9"/>
        <v-container>
            <v-row>
                <v-col>
                    <v-card>
                        <v-treeview :items="viewed_permission" open-on-click hoverable/>
                    </v-card>
                </v-col>
            </v-row>
        </v-container>
    </div>
</template>
<script>
import {Permisssion_Type,Permisson_List} from "@/utils/permission"
import mixin from '@/utils/mixins'
export default {
    name:"Permissions-Main",
    mixins:[mixin],
    data:()=>({
        selfpermissions:null,
        viewed_permission:[],
        Permisssion_Type,
        Permisson_List,
    }),
    watch:{
        selfpermissions:function(nsp){
            if(nsp!==null)this.initializePL();
        }
    },
    methods:{
        initializePL:function(){
            this.viewed_permission=[]
            var cnt=0,j;
            for(var i=0;i<this.selfpermissions.length;i++){
                var p=this.selfpermissions[i];
                var vp={
                    id:cnt++,
                    name:"权限： "+this.Permisson_List[p.type].name,
                };
                if(p.pObject===null)
                {
                    vp.children=[{name:"生效对象：全局"}]
                }
                else
                {
                    vp.children=[];
                    for(j=0;j<p.pObject.length;j++){
                        vp.children.push({
                            id:cnt,
                            name:"生效对象："+(this.Permisson_List[p.type].global===true?"全局":"")+(this.Permisson_List[p.type].committee===true?"会场：":"")+(this.Permisson_List[p.type].filegroup===true?"文件组：":"")+p.pObject[j].pObjectName
                        })
                        cnt+=2;
                    }
                }
                this.viewed_permission.push(vp);
            }
        },
        getSelfPermissions:function(){
            var vm=this;
            vm.selfpermissions=null;
            this.fetchGet('/api/permission')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.selfpermissions=json;
                    })
                }
            })
        }
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getSelfPermissions())
    },
    beforeRouteUpdate (to, from, next) {
        this.getSelfPermissions();
        next()
    },
}
</script>