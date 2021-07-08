<template>
    <div>
        <div class="text-h5 mx-6 my-6">全局设置</div>
        <v-divider class="mx-9"/>
        <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" v-if="enabledPages.enableGlobalSettings!==true || settings===null" >
            您没有查看的权限
        </div>
        <v-container v-else>
            <v-row>
                <v-col sm="4" cols="12">
                    <v-dialog width="500" >
                        <template v-slot:activator="{ on, attrs }">
                            <v-text-field label="报名截止日期" v-model="settings.regPaymentDeadline"  v-bind="attrs" v-on="on" readonly/>
                        </template>
                        <v-date-picker v-model="settings.regPaymentDeadline"/>
                    </v-dialog>
                </v-col>
                <v-col sm="4" cols="12">
                    <v-dialog width="500" >
                        <template v-slot:activator="{ on, attrs }">
                            <v-text-field label="会议开始日期" v-model="settings.conferenceBeginDate"  v-bind="attrs" v-on="on" readonly/>
                        </template>
                        <v-date-picker v-model="settings.conferenceBeginDate"/>
                    </v-dialog>
                </v-col>
                <v-col sm="4" cols="12">
                    <v-dialog width="500" >
                        <template v-slot:activator="{ on, attrs }">
                            <v-text-field label="会议截止日期" v-model="settings.conferenceEndDate"  v-bind="attrs" v-on="on" readonly/>
                        </template>
                        <v-date-picker v-model="settings.conferenceEndDate"/>
                    </v-dialog>
                </v-col>
            </v-row>
            <v-row>
                <v-col md="3" sm="6" cols="12">
                    <v-text-field label="标准住宿费" v-model="settings.standardAccPrice"/>
                </v-col>
                <v-col md="3" sm="6" cols="12">
                    <v-text-field label="延长住宿单价" v-model="settings.extendedAccDailyPrice"/>
                </v-col>
                <v-col md="3" sm="6" cols="12">
                    <v-text-field label="标准住宿退会退款" v-model="settings.standardAccRefund"/>
                </v-col>
                <v-col md="3" sm="6" cols="12">
                    <v-text-field label="延长住宿退会退款单价" v-model="settings.extendedAccDailyRefund"/>
                </v-col>
            </v-row>
            <v-row>
                <v-col />
                <v-col cols="auto">
                    <v-btn outlined color="primary" :disabled="editable!==true" @click.stop="saveSettings()">保存</v-btn>
                </v-col>
            </v-row>
            <v-row>
                <v-col/>
                <v-col cols="auto">
                    <v-btn @click="downloadFile()">导出</v-btn>
                </v-col>
                <v-col/>
            </v-row>
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
        </v-container>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Global-Settings",
    mixins:[mixin],
    data:()=>({
        editable:false,
        notedialog:{
            active:null,
            success:true,
        },
        downloadingFileDialog:null,
        settings:null,
    }),
    methods:{
        getSettings:function(){
            var vm=this;
            this.fetchGet('/api/global/settings')
            .then(function(res){
                if(res.status==200){
                    res.json().then(function(json){
                        json.settings.regPaymentDeadline=json.settings.regPaymentDeadline.split(" ")[0]
                        json.settings.conferenceBeginDate=json.settings.conferenceBeginDate.split(" ")[0]
                        json.settings.conferenceEndDate=json.settings.conferenceEndDate.split(" ")[0]
                        vm.settings=json.settings;
                        vm.editable=json.editable;
                    })
                }
            })
        },
        saveSettings:function(){
            if(this.editable!==true)return;
            var vm=this;
            this.fetchPostJson('/api/global/settings',vm.settings)
            .then(function(res){
                if(res.status==204){
                    vm.notedialog.success=true;
                    vm.notedialog.active=true;
                }
                else{
                    vm.notedialog.success=false;
                    vm.notedialog.active=true;
                }
            })
        },
        downloadFile:async function(){
            this.downloadingFileDialog=true;
            // TODO: Fetch
            var suc=await this.fetchDownload('/api/global/export')
            if(suc!==true){
                this.notedialog.success=false;
                this.notedialog.active=true;
            }
            this.downloadingFileDialog=false;
            //this.downloadingFileDialog=false;
        },
        fetchDownload:async function(url){
            let response = await this.fetchGet(url);
            if(response.status!==200)return false;
            const filename = response.headers.get('content-disposition').split(';')[1].split('=')[1]
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
    },
    beforeRouteEnter (to, from, next) {
        next(vm => vm.getSettings())
    },
    beforeRouteUpdate (to, from, next) {
        this.getSettings();
        next()
    },
}
</script>