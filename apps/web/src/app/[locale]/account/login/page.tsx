import { notFound } from "next/navigation";import{AccountForm}from"@/components/account-form";import{hasLocale}from"@/i18n/config";
export default async function MemberLogin({params}:PageProps<"/[locale]/account/login">){const{locale}=await params;if(!hasLocale(locale))notFound();return <main className="account-page"><AccountForm locale={locale} mode="login"/></main>}
