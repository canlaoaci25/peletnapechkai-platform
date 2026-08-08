"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import type { ManagedUser } from "@/lib/admin-api";

const allRoles = ["Admin", "Editor", "Author", "Translator", "SEO"];
type Copy = Record<string, string>;

export function UserManager({ users, copy }: { users: ManagedUser[]; copy: Copy }) {
  const router = useRouter(); const [message, setMessage] = useState(""); const [invitation, setInvitation] = useState<{userId:string; invitationToken:string}|null>(null);
  async function request(path:string, method:string, body?:object) { const csrf=await fetch("/api/admin/auth/csrf",{cache:"no-store"}); const {token}=await csrf.json() as {token:string}; const response=await fetch(`/api/admin/users${path}`,{method,headers:{"content-type":"application/json","x-csrf-token":token},body:body?JSON.stringify(body):undefined}); if(!response.ok) throw new Error(); return response; }
  async function invite(event:FormEvent<HTMLFormElement>){event.preventDefault();setMessage("");try{const data=new FormData(event.currentTarget);const response=await request("/invite","POST",{email:data.get("email"),displayName:data.get("displayName"),roles:data.getAll("roles")});setInvitation(await response.json());event.currentTarget.reset();router.refresh()}catch{setMessage(copy.error)}}
  async function roles(event:FormEvent<HTMLFormElement>,id:string){event.preventDefault();try{const data=new FormData(event.currentTarget);await request(`/${id}/roles`,"PUT",{roles:data.getAll("roles")});setMessage(copy.success);router.refresh()}catch{setMessage(copy.error)}}
  async function action(id:string,path:string,method="POST",body?:object){try{await request(`/${id}/${path}`,method,body);setMessage(copy.success);router.refresh()}catch{setMessage(copy.error)}}
  return <div className="user-manager"><form className="admin-form" onSubmit={invite}><h2>{copy.invite}</h2><label>{copy.name}<input name="displayName" required /></label><label>{copy.email}<input name="email" type="email" required /></label><fieldset><legend>{copy.roles}</legend>{allRoles.map(role=><label className="check-label" key={role}><input type="checkbox" name="roles" value={role}/>{role}</label>)}</fieldset><button>{copy.send}</button></form>{invitation&&<aside className="invitation-result"><h2>{copy.tokenTitle}</h2><p>{copy.tokenWarning}</p><code>{JSON.stringify(invitation)}</code></aside>}<div className="user-list">{users.map(user=><article className="user-card" key={user.id}><header><div><strong>{user.displayName}</strong><small>{user.email}</small></div><span>{user.isActive?copy.active:copy.inactive}</span></header><form onSubmit={event=>roles(event,user.id)}><fieldset><legend>{copy.roles}</legend>{allRoles.map(role=><label className="check-label" key={role}><input type="checkbox" name="roles" value={role} defaultChecked={user.roles.includes(role)}/>{role}</label>)}</fieldset><button>{copy.saveRoles}</button></form><div className="workflow-actions"><button className="button-secondary" onClick={()=>action(user.id,"active","PUT",{isActive:!user.isActive})}>{user.isActive?copy.disable:copy.enable}</button><button className="button-secondary" onClick={()=>action(user.id,"revoke-sessions")}>{copy.revoke}</button></div></article>)}</div>{message&&<p role="status">{message}</p>}</div>;
}
