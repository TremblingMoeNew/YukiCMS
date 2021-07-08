<template>
    <div>
        <v-tabs show-arrows color="orange lighten-1" v-model="tab">
            <v-tab>作为学测官的</v-tab>
            <v-tab>未指定学测官的</v-tab>
            <v-tab>会场学测归档</v-tab>
        </v-tabs>
        <v-tabs-items v-model="tab">
            <v-tab-item>
                <v-data-table :headers="reviews_header" :items="reviewsSelfAdm" @click:row="expandSA" class="mx-12 my-5" calculate-widths :loading="loading" loading-text="加载中……" :expanded.sync="expandedSA" show-expand single-expand  item-key="tid" sort-by="tid" sort-desc>
                    <template v-slot:expanded-item="{ headers, item }">
                        <td :colspan="headers.length"  @click="$router.push({name:'Committee-Review-Main',params:{cid:item.cid,tid:item.tid}})">
                            <v-card flat class="my-1">
                                <v-card-subtitle>问题:</v-card-subtitle>
                                <v-card-text>       
                                    <p v-for="(question,j) of item.questions" :key="j" class="mx-3 my-1">{{j+1}}： {{question}}</p>
                                </v-card-text>
                                <v-card-subtitle>点评:</v-card-subtitle>
                                <v-card-text>
                                    <p class="mx-3 my-1">{{item.comment}}</p>
                                </v-card-text>
                                <p class="d-flex justify-center grey--text lighten-1 align-self-auto">
                                    点击以查看回答
                                </p>
                            </v-card>
                        </td>
                    </template>
                    <template v-slot:item.data-table-expand />
                </v-data-table>
            </v-tab-item>
            <v-tab-item>
                <v-data-table :headers="reviews_header" :items="reviewsGeneralAdm" @click:row="expandGA" class="mx-12 my-5" calculate-widths :loading="loading" loading-text="加载中……" :expanded.sync="expandedGA" show-expand single-expand  item-key="tid" sort-by="tid" sort-desc>
                    <template v-slot:expanded-item="{ headers, item }">
                        <td :colspan="headers.length" @click="$router.push({name:'Committee-Review-Main',params:{cid:item.cid,tid:item.tid}})">
                            <v-card flat class="my-1">
                                <v-card-subtitle>问题:</v-card-subtitle>
                                <v-card-text>       
                                    <p v-for="(question,j) of item.questions" :key="j" class="mx-3 my-1">{{j+1}}： {{question}}</p>
                                </v-card-text>
                                <v-card-subtitle>点评:</v-card-subtitle>
                                <v-card-text>
                                    <p class="mx-3 my-1">{{item.comment}}</p>
                                </v-card-text>
                                <p class="d-flex justify-center grey--text lighten-1 align-self-auto">
                                    点击以查看回答
                                </p>
                            </v-card>
                        </td>
                    </template>
                    <template v-slot:item.data-table-expand />
                </v-data-table>
            </v-tab-item>
            <v-tab-item>
                <v-data-table :headers="reviews_header" :items="reviews" @click:row="expandAll" class="mx-12 my-5" calculate-widths :loading="loading" loading-text="加载中……" :expanded.sync="expandedAll" show-expand single-expand  item-key="tid" sort-by="tid" sort-desc>
                    <template v-slot:expanded-item="{ headers, item }">
                        <td :colspan="headers.length" @click="$router.push({name:'Committee-Review-Main',params:{cid:item.cid,tid:item.tid}})">
                            <v-card flat class="my-1">
                                <v-card-subtitle>问题:</v-card-subtitle>
                                <v-card-text>       
                                    <p v-for="(question,j) of item.questions" :key="j" class="mx-3 my-1">{{j+1}}： {{question}}</p>
                                </v-card-text>
                                <v-card-subtitle>点评:</v-card-subtitle>
                                <v-card-text>
                                    <p class="mx-3 my-1">{{item.comment}}</p>
                                </v-card-text>
                                <p class="d-flex justify-center grey--text lighten-1 align-self-auto">
                                    点击以查看回答
                                </p>
                            </v-card>
                        </td>
                    </template>
                    <template v-slot:item.data-table-expand />
                </v-data-table>
            </v-tab-item>
        </v-tabs-items>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Reviews-Management",
    mixins:[mixin],
    props:{
        reviewsAsReviewer:Array,
        rga:Array,
        rarchived:Array,
        permission:Object
    },
    data:()=>({
        tab:null,
        expandedSA:[],
        expandedGA:[],
        expandedAll:[],
        loading:false,
        reviews_header:[
            {text:'学测编号',value:'tid'},
            {text:'代表姓名',value:'uname'},
            {text:'学测官',value:'admName'},
            {text:'截止时间',value:'ddl'},
            {text:'问题数',value:'questions.length'},
            {text:'状态',value:'status_str'}
        ],
        reviewsSelfAdm:[],
        reviewsGeneralAdm:[],
        reviews:[],
        const_Review_Status:[
            {text:"未完成",color:"deep-orange--text accent-2"},
            {text:"已提交",color:"blue--text lighten-4"},
            {text:"已完成",color:"green--text darken-1"},
            {text:"已过期",color:"deep-purple--text darken-1"},
            {text:"已关闭",color:"grey--text darken-1"},
        ],
        rga:[],
        rarchived:[]
    }),
    watch:{
        reviewsAsReviewer:function(){
            this.initReviewsSelfAdm();
        },
        rga:function(){
            this.initReviewsGeneralAdm();
        },
        rarchived:function(){
            this.initReviewsArchived();
        }
    },
    methods:{
        expandSA: function(event){
            this.expandedSA=((this.expandedSA!==null&&this.expandedSA.length>0&&this.expandedSA[0].tid===event.tid)?[]:[event])
        },
        expandGA: function(event){
            this.expandedGA=((this.expandedGA!==null&&this.expandedGA.length>0&&this.expandedGA[0].tid===event.tid)?[]:[event])
        },
        expandAll: function(event){
            this.expandedAll=((this.expandedAll!==null&&this.expandedAll.length>0&&this.expandedAll[0].tid===event.tid)?[]:[event])
        },
        initReview:async function(review){
            var vm=this;
            if(review.admUid==0){
                review.admName="未指定"
            }
            review.status_str=vm.const_Review_Status[review.status].text;
            review.questions_str=[];
        },
        initReviewsSelfAdm:async function(){
            var vm=this;
            vm.reviewsSelfAdm=[]
            for(var i=0;i<vm.reviewsAsReviewer.length;i++){
                var review=Object.assign({},vm.reviewsAsReviewer[i]);
                await vm.initReview(review);
                vm.reviewsSelfAdm.push(review);
            }
        },
        initReviewsGeneralAdm:async function(){
            var vm=this;
            vm.reviewsGeneralAdm=[]
            for(var i=0;i<vm.rga.length;i++){
                var review=Object.assign({},vm.rga[i]);
                await vm.initReview(review);
                vm.reviewsGeneralAdm.push(review);
            }
        },
        initReviewsArchived:async function(){
            var vm=this;
            vm.reviews=[]
            for(var i=0;i<vm.rarchived.length;i++){
                var review=Object.assign({},vm.rarchived[i]);
                await vm.initReview(review);
                vm.reviews.push(review);
            }
        },
    },
    created:function(){
        this.initReviewsSelfAdm();
        this.initReviewsGeneralAdm();
        this.initReviewsArchived();
    }
}
</script>