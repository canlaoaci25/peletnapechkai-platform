import { notFound } from "next/navigation";import{AccountForm}from"@/components/account-form";import{hasLocale}from"@/i18n/config";
export default async function MemberRegister({params}:PageProps<"/[locale]/account/register">){const{locale}=await params;if(!hasLocale(locale))notFound();return <main className="account-page"><AccountForm locale={locale} mode="register"/></main>}
