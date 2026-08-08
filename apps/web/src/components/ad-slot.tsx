"use client";
import {useEffect,useState} from "react";
export function AdSlot({label}:{label:string}){const [allowed,setAllowed]=useState(false);useEffect(()=>{const update=()=>setAllowed(localStorage.getItem("boecl-consent")==="granted");const timer=setTimeout(update,0);window.addEventListener("storage",update);return()=>{clearTimeout(timer);window.removeEventListener("storage",update)}},[]);if(process.env.NEXT_PUBLIC_ADS_ENABLED!=="true"||!allowed)return null;return <aside className="ad-slot" aria-label={label} data-placement="managed"><span>{label}</span></aside>}
