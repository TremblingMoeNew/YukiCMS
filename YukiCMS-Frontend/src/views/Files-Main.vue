<template>
    <div>
        <div class="text-h5 mx-6 mt-6">文件</div>
        <div class="d-flex align-center mx-9">
            <v-tabs v-model="tab" right center-active show-arrows>
                <v-tab>下载</v-tab>
                <v-tab v-if="upload_group!==null && upload_group.length!==0">管理</v-tab>
            </v-tabs>
        </div>
        <v-divider class="mx-9"/>
        <v-tabs-items v-model="tab">
            <v-tab-item>
                <div class="mx-2 my-5">
                    <v-container>
                        <v-row>
                            <v-col v-for="(grp,idx) of download_group" :key="idx" cols="12" lg="4" md="6" align-self="center">
                            <v-card>
                                <v-card-title  class="mb-1 pb-1">{{grp.name}}</v-card-title>
                                <v-card-text >
                                    <v-divider class="mx-3" />
                                    <div class="d-flex my-3 justify-center grey--text lighten-1 text-subtitle-1 align-self-auto" v-if="grp.filenames===undefined || grp.filenames===null || grp.filenames.length===0">
                                        暂无文件
                                    </div>
                                    <p v-for="(name,j) of grp.filenames" :key="j" class="mx-md-2 my-0">
                                        <v-btn text  @click="downloadFile(grp.fgid,name)" class="text-body-1" color="blue">{{name.fileClaimedName}}</v-btn>
                                    </p>
                                </v-card-text>
                            </v-card>
                            </v-col>
                        </v-row>
                    </v-container>
                </div>
            </v-tab-item>
            <v-tab-item>
                <div class="mx-2 my-5">
                    <v-container>
                        <v-row>
                            <v-col v-for="(grp,idx) of upload_group" :key="idx" cols="12" lg="4" md="6" align-self="center">
                                <v-card>
                                    <v-card-title  class="mb-1 pb-1">{{grp.name}}</v-card-title>
                                    <v-card-text >
                                        <v-divider class="mx-3" />
                                        <div class="d-flex my-3 justify-center grey--text lighten-1 text-subtitle-1 align-self-auto" v-if="grp.filenames===undefined || grp.filenames===null || grp.filenames.length===0">
                                            暂无文件
                                        </div>
                                        <v-container v-else>
                                            <v-row v-for="(name,j) of grp.filenames" :key="j" class="my-0 py-0">
                                                <v-col cols="auto" class="my-0 py-0">
                                                    <v-btn text color="deep-orange accent-2" class="my-0 py-0" @click.stop="openDeleteFileDialog(grp,name)">—</v-btn>
                                                </v-col>
                                                <v-col class="my-0 py-0" align-self="center">{{name.fileClaimedName}}</v-col>
                                            </v-row>
                                        </v-container>
                                        <div class="d-flex">
                                            <v-spacer />
                                            <v-btn outlined color="primary" @click.stop="openUploadDialog(grp)">上传</v-btn>
                                        </div>
                                    </v-card-text>
                                </v-card>
                            </v-col>
                        </v-row>
                    </v-container>
                </div>
            </v-tab-item>
        </v-tabs-items>
        <v-dialog v-model="notedialog.active" width="200">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text>
                    <p v-if="notedialog.success===true" class="d-flex justify-center">操作成功！</p>
                    <p v-else class="d-flex justify-center">操作失败！</p>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-dialog v-model="downloadingFileDialog" width="400" persistent>
            <v-card class="pt-5">
                <v-card-text>
                    <div class="mx-12">
                        <p class="d-flex justify-center">下载中……</p>
                        <v-progress-linear indeterminate/>
                    </div>
                </v-card-text>
            </v-card>
        </v-dialog>
        <v-dialog v-model="deleteFileDialog" width="400">
            <v-card>
                <v-card-title>提示</v-card-title>
                <v-card-text class="d-flex justify-center">
                    <p>
                        您确定要删除文件 <span class="deep-orange--text accent-2">{{activegroup.filename.fileClaimedName}}</span> 吗？
                    </p>
                </v-card-text>
                <v-card-actions>
                    <v-spacer />
                    <v-btn text @click="deleteFileDialog=false;">取消</v-btn>
                    <v-btn text color="deep-orange accent-2" @click="deleteFile()">确定</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <v-dialog v-model="uploadFileDialog" width="400" :persistent="ufdPersistent">
            <v-card>
                <v-card-title>上传文件</v-card-title>
                <v-card-subtitle>点击以上传文件</v-card-subtitle>
                <v-card-text>
                    <p v-if="uploadsizeExceeded" class="d-flex justify-center deep-orange--text accent-2">文件大小不得大于12MB</p>
                    <div class="mx-12" v-if="uploading===true">
                        <p class="d-flex justify-center">上传中……</p>
                        <v-progress-linear indeterminate/>
                    </div>
                    <v-file-input v-model="uploadvalue" @change="uploadFile" show-size label="上传文件"/>
                </v-card-text>
            </v-card>
        </v-dialog>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"File-Main",
    mixins:[mixin],
    data:()=>({
        tab:null,
        activegroup:{
            fg:null,
            filename:{},
        },
        deleteFileDialog:null,
        uploadFileDialog:null,
        ufdPersistent:false,
        uploading:false,
        uploadsizeExceeded:false,
        uploadvalue:null,
        notedialog:{
            active:null,
            success:true,
        },
        downloadingFileDialog:null,
        downloadvalue:0,
        download_group:[],
        upload_group:[],
    }),
    computed:{
        downloadpercent:function(){
            return this.downloadvalue.cur *100 / this.downloadvalue.total
        }
    },
    watch:{
    },
    methods:{
        downloadFile:async function(fgid,filename){
            this.downloadingFileDialog=true;
            // TODO: Fetch
            var suc=await this.fetchDownload('/api/filegroup/'+fgid+'/file/'+filename.fileStorageName,filename.fileOriginalName)
            if(suc!==true){
                this.notedialog.success=false;
                this.notedialog.active=true;
            }
            this.downloadingFileDialog=false;
            //this.downloadingFileDialog=false;
        },
        fetchDownload:async function(url,filename){
            let response = await this.fetchGet(url);
            if(response.status!==200)return false;
            var blob=await response.blob();
            const link = document.createElement('a')
            link.download = decodeURIComponent(filename)
            link.style.display = 'none'
            link.href = URL.createObjectURL(blob)
            link.onload=function(){
                URL.revokeObjectURL(link.href)
            }
            link.click()
            return true;
        },
        openDeleteFileDialog:function(fg,filename){
            this.activegroup.fg=fg;
            this.activegroup.filename=filename;
            this.deleteFileDialog=true;
        },
        deleteFile:function(){
            var vm=this;
            this.fetchDelete('/api/filegroup/'+this.activegroup.fg.fgid+'/file/'+this.activegroup.filename.fileStorageName)
            .then(function(res){
                if(res.status===204){
                    vm.initGetGroups()
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
            this.deleteFileDialog=false;
        },
        openUploadDialog:function(fg){
            this.activegroup.fg=fg;
            this.uploadvalue=null;
            this.uploadFileDialog=true;
        },
        uploadFile:function(event){
            this.uploadsizeExceeded=event.size>1024*1024*12;
            if(this.uploadsizeExceeded)return;
            this.ufdPersistent=true;
            this.uploading=true;
            var vm=this;
            this.fetchUploadPost('/api/filegroup/'+this.activegroup.fg.fgid+'/file',event)
            .then(function(res){
                if(res.status===200){
                    vm.initGetGroups()
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=false;
                }
                vm.uploading=false;
                vm.ufdPersistent=false;
            })
        },
        initGetGroups:function(){
            this.getDownloadGroup();
            this.getUploadGroup();
        },
        getDownloadGroup:function(){
            var vm=this;
            this.download_group=[]
            this.fetchGet('/api/filegroup/')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.getDGFileNames(json);
                        vm.download_group=json;
                    })
                }
            })
        },
        getUploadGroup:function(){
            var vm=this;
            this.upload_group=[]
            this.fetchGet('/api/filegroup/upload')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        vm.getUGFileNames(json);    
                        vm.upload_group=json;
                    })
                }
            })
        },
        getFileNames:function(grp){
            var vm=this;
            grp.filenames=null
            vm.fetchGet('/api/filegroup/'+grp.fgid+'/file')
            .then(function(res){
                if(res.status===200){
                res.json().then(function(json){
                        grp.filenames=json;
                    })
                }
            })
        },
        getDGFileNames:function(dg){
            var vm=this;
            for(var i=0;i<dg.length;i++)
            {
                var grp=dg[i];
                vm.getFileNames(grp);
            }
        },
        getUGFileNames:function(ug){
            var vm=this;
            for(var i=0;i<ug.length;i++)
            {
                var grp=ug[i];
                vm.getFileNames(grp);
            }
        },
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.initGetGroups())
    },
    beforeRouteUpdate (to, from, next) {
        this.initGetGroups();
        next()
    },
}
</script>