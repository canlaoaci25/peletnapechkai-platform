import assert from "node:assert/strict";
import test from "node:test";
import { buildPlanningClusters, planningHubSlugs } from "./planning-authority-hub.ts";
import { readFileSync } from "node:fs";

const article=(slug:string,sourceCount:number,reviewedSourceCount:number)=>({articleGroupId:slug,slug,title:slug,summary:"Özet",type:"Guide",publishedAt:"2026-08-18",updatedAt:"2026-08-18",cover:null,sourceCount,reviewedSourceCount});
test("recognizes every localized authority route",()=>assert.deepEqual(Object.keys(planningHubSlugs),["tr-TR","en-US","de-DE","fr-FR"]));
test("groups each story once and ranks visible evidence first",()=>{
  const clusters=buildPlanningClusters("tr-TR",[article("forest-odak",1,0),article("focusmate-rehberi",3,2),article("clockify-inceleme",2,1)]);
  assert.deepEqual(clusters.flatMap(cluster=>cluster.articles.map(item=>item.slug)),["focusmate-rehberi","forest-odak","clockify-inceleme"]);
});
test("authority hub publishes absolute CollectionPage discovery and visible source evidence",()=>{
  const page=readFileSync(new URL("../app/[locale]/[collection]/[slug]/page.tsx",import.meta.url),"utf8");
  assert.match(page,/"@type":"CollectionPage"/);
  assert.match(page,/url:absoluteUrl\(`/);
  assert.match(page,/article\.sourceCount/);
});
