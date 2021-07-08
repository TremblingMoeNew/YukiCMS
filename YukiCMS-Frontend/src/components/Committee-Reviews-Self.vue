<template>
    <div>
        <v-container fluid class="d-flex">
            <v-row justify="center" no-gutters>
                <v-col order="1" order-lg="3" lg="4" cols="12">
                    <v-card class="mx-2 my-2">
                        <v-card-title>完成情况</v-card-title>
                        <v-card-text>
                            <div v-if="statusCount!==null">
                                <p v-for="(item,idx) of const_Review_Status" :key="idx" class="mx-5 my-1" >
                                    <template v-if="statusCount.length>idx">
                                        {{item.text}}：
                                        <span :class="item.color">{{statusCount[idx]}}</span>
                                    </template>
                                </p>
                            </div>
                        </v-card-text>
                    </v-card>
                </v-col>
                <v-col order="2" lg="auto" cols="12">
                    <v-divider class="d-none d-lg-flex mx-5" vertical/>
                    <v-divider class="d-flex d-lg-none my-5"/> 
                </v-col>
                <v-col order="3" order-lg="1">
                    <div class="d-flex my-9 py-9 justify-center grey--text lighten-1 text-h6 align-self-auto" v-if="reviewsSelfView===null||reviewsSelfView.length===0">您当前没有等待完成的学测</div>
                    <v-card v-for="(review,idx) of reviewsSelfView" :key="idx" class="mx-3 my-3"  @click="$router.push({name:'Committee-Review-Main',params:{cid:review.cid,tid:review.tid}})">
                        <v-card-text>
                            <p v-for="(question,j) of review.questions" :key="j" class="mx-3 my-1">{{j+1}}： {{question}}</p>
                        </v-card-text>
                        <v-card-subtitle>
                            状态: 
                            <span :class="const_Review_Status[review.status].color"> {{const_Review_Status[review.status].text}} </span>
                            / 截止时间：{{review.ddl}}
                        </v-card-subtitle>
                    </v-card>
                </v-col>
            </v-row>
        </v-container>
    </div>
</template>
<script>
import mixin from '@/utils/mixins'
export default {
    name:"Committee-Reviews-Self",
    mixins:[mixin],
    props:['reviewsSelf'],
    data:()=>({
        statusCount:[
            2,3,2,1,2
        ],
        const_Review_Status:[
            {text:"未完成",color:"deep-orange--text accent-2"},
            {text:"已提交",color:"blue--text lighten-4"},
            {text:"已完成",color:"green--text darken-1"},
            {text:"已过期",color:"deep-purple--text darken-1"},
            {text:"已关闭",color:"grey--text darken-1"},
        ],
        reviewsSelfView:[

        ],
    }),
    watch:{
        reviewsSelf:function(){
            this.initReviewsSelf()
        }
    },
    methods:{
        initReviewsSelf:async function(){
            var vm=this;
            vm.statusCount=[0,0,0,0,0]
            vm.reviewsSelfView=[]
            for(var i=0;i<vm.reviewsSelf.length;i++){
                var review=Object.assign({},vm.reviewsSelf[i]);
                vm.statusCount[review.status]++;
                vm.reviewsSelfView.push(review);
            }
        }
    }
}
</script>